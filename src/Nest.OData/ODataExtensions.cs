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
            return node.OperatorKind switch
            {
                BinaryOperatorKind.And => AggregateAndOperations(node),
                BinaryOperatorKind.Or => AggregateOrOperations(node),
                BinaryOperatorKind.Equal => (QueryContainer)new TermQuery { Field = ExtractFieldName(node.Left), Value = ExtractValue(node.Right) },
                BinaryOperatorKind.NotEqual => (QueryContainer)!new TermQuery { Field = ExtractFieldName(node.Left), Value = ExtractValue(node.Right) },
                BinaryOperatorKind.GreaterThan => (QueryContainer)new TermRangeQuery { Field = ExtractFieldName(node.Left), GreaterThan = ExtractValue(node.Right) },
                BinaryOperatorKind.LessThan => (QueryContainer)new TermRangeQuery { Field = ExtractFieldName(node.Left), LessThan = ExtractValue(node.Right) },
                _ => throw new NotImplementedException($"Unsupported binary operator: {node.OperatorKind}"),
            };
        }

        private static QueryContainer AggregateOrOperations(BinaryOperatorNode node)
        {
            var queries = new List<QueryContainer>();

            void Collect(QueryNode queryNode)
            {
                if (queryNode is BinaryOperatorNode binaryNode && binaryNode.OperatorKind == BinaryOperatorKind.Or)
                {
                    Collect(binaryNode.Left);
                    Collect(binaryNode.Right);
                }
                else
                {
                    queries.Add(TranslateExpression(queryNode));
                }
            }

            Collect(node);

            return new BoolQuery { Should = queries, MinimumShouldMatch = 1 };
        }

        private static QueryContainer AggregateAndOperations(BinaryOperatorNode node)
        {
            var queries = new List<QueryContainer>();

            void Collect(QueryNode queryNode)
            {
                if (queryNode is BinaryOperatorNode binaryNode && binaryNode.OperatorKind == BinaryOperatorKind.And)
                {
                    Collect(binaryNode.Left);
                    Collect(binaryNode.Right);
                }
                else
                {
                    queries.Add(TranslateExpression(queryNode));
                }
            }

            Collect(node);

            return new BoolQuery { Must = queries };
        }

        private static QueryContainer TranslateSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
        {
            var field = ExtractFieldName(node.Parameters.First());
            var value = ExtractValue(node.Parameters.Last());

            return node.Name.ToLower() switch
            {
                "startswith" => new PrefixQuery { Field = field, Value = value },
                "endswith" => new WildcardQuery { Field = field, Value = $"*{value}" },
                "contains" => new WildcardQuery { Field = field, Value = $"*{value}*" },
                "substringof" => new MatchQuery { Field = field, Query = value },
                _ => throw new NotImplementedException($"Unsupported function: {node.Name}"),
            };
        }

        private static string ExtractFieldName(QueryNode node)
        {
            var segments = new List<string>();

            void ProcessNode(QueryNode currentNode)
            {
                switch (currentNode)
                {
                    case SingleValuePropertyAccessNode propertyAccessNode:
                        segments.Insert(0, propertyAccessNode.Property.Name);
                        ProcessNode(propertyAccessNode.Source);
                        break;
                    case SingleNavigationNode navigationNode:
                        segments.Insert(0, navigationNode.NavigationProperty.Name);
                        ProcessNode(navigationNode.Source);
                        break;
                }
            }

            ProcessNode(node);

            if (segments.Count == 0)
            {
                throw new NotImplementedException("No field name could be extracted.");
            }

            return string.Join(".", segments);
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
