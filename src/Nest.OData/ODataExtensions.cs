﻿using Microsoft.AspNetCore.OData.Query;
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
                QueryNodeKind.BinaryOperator => TranslateOperatorNode(node as BinaryOperatorNode),
                QueryNodeKind.SingleValueFunctionCall => TranslateFunctionCallNode(node as SingleValueFunctionCallNode),
                QueryNodeKind.Convert => TranslateExpression(((ConvertNode)node).Source),
                QueryNodeKind.SingleValuePropertyAccess => null, 
                QueryNodeKind.Constant => null,
                _ => throw new NotImplementedException($"Unsupported node type: {node.Kind}"),
            };
        }

        private static QueryContainer TranslateOperatorNode(BinaryOperatorNode node)
        {
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(node.Left);
            var fieldName = GetFieldName(fullyQualifiedFieldName);

            var query = node.OperatorKind switch
            {
                BinaryOperatorKind.And => AggregateAndOperations(node),
                BinaryOperatorKind.Or => AggregateOrOperations(node),
                BinaryOperatorKind.Equal => new TermQuery { Field = fieldName, Value = ExtractValue(node.Right) },
                BinaryOperatorKind.NotEqual => !new TermQuery { Field = fieldName, Value = ExtractValue(node.Right) },
                BinaryOperatorKind.GreaterThan => new TermRangeQuery { Field = fieldName, GreaterThan = ExtractValue(node.Right) },
                BinaryOperatorKind.LessThan => new TermRangeQuery { Field = fieldName, LessThan = ExtractValue(node.Right) },
                _ => throw new NotImplementedException($"Unsupported binary operator: {node.OperatorKind}"),
            };

            if ((node.Left as SingleValuePropertyAccessNode).Source is SingleNavigationNode)
            {
                return new NestedQuery
                {
                    Path = GetNestedPath(fullyQualifiedFieldName),
                    Query = query,
                };
            }

            return query;
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

        private static QueryContainer TranslateFunctionCallNode(SingleValueFunctionCallNode node)
        {
            var left = node.Parameters.First();
            var right = node.Parameters.Last();
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(left);
            var fieldName = GetFieldName(fullyQualifiedFieldName);
            var value = ExtractValue(right);

            var query = node.Name.ToLower() switch
            {
                "startswith" => (QueryContainer)new PrefixQuery { Field = fieldName, Value = value },
                "endswith" => new WildcardQuery { Field = fieldName, Value = $"*{value}" },
                "contains" => new WildcardQuery { Field = fieldName, Value = $"*{value}*" },
                "substringof" => new MatchQuery { Field = fieldName, Query = value },
                _ => throw new NotImplementedException($"Unsupported function: {node.Name}"),
            };

            if ((left as SingleValuePropertyAccessNode).Source is SingleNavigationNode)
            {
                return new NestedQuery
                {
                    Path = GetNestedPath(fullyQualifiedFieldName),
                    Query = query,
                };
            }

            return query;
        }

        private static string ExtractFullyQualifiedFieldName(QueryNode node)
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

        private static string GetNestedPath(string fullyQualifiedFieldName)
        {
            var lastIndex = fullyQualifiedFieldName.LastIndexOf('.');
            return lastIndex > 0 ? fullyQualifiedFieldName[..lastIndex] : fullyQualifiedFieldName;
        }

        private static string GetFieldName(string fullyQualifiedFieldName)
        {
            var lastIndex = fullyQualifiedFieldName.LastIndexOf('.') + 1;
            return lastIndex > 0 ? fullyQualifiedFieldName[lastIndex..] : fullyQualifiedFieldName;
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
