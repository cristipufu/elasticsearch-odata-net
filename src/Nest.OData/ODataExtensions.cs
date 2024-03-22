using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

#nullable disable
namespace Nest.OData
{
    public static class ODataExtensions
    {
        public static QueryContainer ToQueryContainer<T>(this ODataQueryOptions<T> queryOptions)
        {
            if (queryOptions?.Filter?.FilterClause?.Expression == null)
            {
                return null;
            }

            return TranslateExpression(queryOptions.Filter.FilterClause.Expression);
        }

        private static QueryContainer TranslateExpression(QueryNode node)
        {
            return node.Kind switch
            {
                QueryNodeKind.BinaryOperator => TranslateBinaryOperatorNode(node as BinaryOperatorNode),
                QueryNodeKind.SingleValueFunctionCall => TranslateSingleValueFunctionCallNode(node as SingleValueFunctionCallNode),
                QueryNodeKind.Convert => TranslateExpression(((ConvertNode)node).Source),
                QueryNodeKind.SingleValuePropertyAccess => null, 
                QueryNodeKind.Constant => null,
                _ => throw new NotImplementedException($"Unsupported node type: {node.Kind}"),
            };
        }

        private static QueryContainer TranslateBinaryOperatorNode(BinaryOperatorNode node)
        {
            var left = TranslateExpression(node.Left);
            var right = TranslateExpression(node.Right);

            return node.OperatorKind switch
            {
                BinaryOperatorKind.And => (QueryContainer)new BoolQuery { Must = [left, right] },
                BinaryOperatorKind.Or => (QueryContainer)new BoolQuery { Should = [left, right], MinimumShouldMatch = 1 },
                BinaryOperatorKind.Equal => (QueryContainer)new TermQuery { Field = ExtractFieldName(node.Left), Value = ExtractValue(node.Right) },
                BinaryOperatorKind.NotEqual => (QueryContainer)!new TermQuery { Field = ExtractFieldName(node.Left), Value = ExtractValue(node.Right) },
                BinaryOperatorKind.GreaterThan => (QueryContainer)new TermRangeQuery { Field = ExtractFieldName(node.Left), GreaterThan = ExtractValue(node.Right) },
                BinaryOperatorKind.LessThan => (QueryContainer)new TermRangeQuery { Field = ExtractFieldName(node.Left), LessThan = ExtractValue(node.Right) },
                _ => throw new NotImplementedException($"Unsupported binary operator: {node.OperatorKind}"),
            };
        }

        private static QueryContainer TranslateSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
        {
            var field = ExtractFieldName(node.Parameters.First());
            var value = ExtractValue(node.Parameters.Last());

            return node.Name.ToLower() switch
            {
                "startswith" => (QueryContainer)new PrefixQuery { Field = field, Value = value },
                "contains" => (QueryContainer)new WildcardQuery { Field = field, Value = $"*{value}*" },
                _ => throw new NotImplementedException($"Unsupported function: {node.Name}"),
            };
        }

        private static string ExtractFieldName(QueryNode node)
        {
            if (node is SingleValuePropertyAccessNode propertyNode)
            {
                return propertyNode.Property.Name;
            }

            throw new NotImplementedException("Complex field names are not supported yet.");
        }

        private static string ExtractValue(QueryNode node)
        {
            if (node is ConstantNode constantNode)
            {
                return constantNode.Value.ToString();
            }

            throw new NotImplementedException("Complex values are not supported yet.");
        }
    }
}
