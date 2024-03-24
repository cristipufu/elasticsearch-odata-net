using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

#nullable disable
namespace Nest.OData
{
    public static class ODataFilterExtensions
    {
        public static QueryContainer ToQueryContainer<T>(this ODataQueryOptions<T> queryOptions)
        {
            if (queryOptions?.Filter?.FilterClause?.Expression == null)
            {
                return null;
            }

            return TranslateExpression(queryOptions.Filter.FilterClause.Expression);
        }

        internal static QueryContainer TranslateExpression(QueryNode node, ODataExpressionContext context = null)
        {
            return node.Kind switch
            {
                QueryNodeKind.Any => TranslateAnyNode(node as AnyNode),
                QueryNodeKind.All => TranslateAllNode(node as AllNode),
                QueryNodeKind.In => TranslateInNode(node as InNode),
                QueryNodeKind.BinaryOperator => TranslateOperatorNode(node as BinaryOperatorNode, context),
                QueryNodeKind.SingleValueFunctionCall => TranslateFunctionCallNode(node as SingleValueFunctionCallNode, context),
                QueryNodeKind.Convert => TranslateExpression(((ConvertNode)node).Source),
                QueryNodeKind.SingleValuePropertyAccess => null,
                QueryNodeKind.Constant => null,
                _ => throw new NotImplementedException($"Unsupported node type: {node.Kind}"),
            };
        }

        private static QueryContainer TranslateAnyNode(AnyNode node)
        {
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(node.Source);

            var isNavigationProperty = node.Source is CollectionNavigationNode ||
                ((node.Source is CollectionPropertyAccessNode collectionNode) && IsNavigationNode(collectionNode.Source.Kind));

            var query = TranslateExpression(node.Body, new ODataExpressionContext
            {
                PathPrefix = fullyQualifiedFieldName,
            });

            if (isNavigationProperty)
            {
                return new NestedQuery
                {
                    Path = ExtractNestedPath(fullyQualifiedFieldName),
                    Query = query,
                };
            }

            return query;
        }

        private static QueryContainer TranslateAllNode(AllNode node)
        {
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(node.Source);

            var isNavigationProperty = node.Source is CollectionNavigationNode ||
                ((node.Source is CollectionPropertyAccessNode collectionNode) && IsNavigationNode(collectionNode.Source.Kind));

            var query = new BoolQuery
            {
                MustNot = [
                    !TranslateExpression(node.Body, new ODataExpressionContext
                    {
                        PathPrefix = fullyQualifiedFieldName,
                    })
                ]
            };

            if (isNavigationProperty)
            {
                return new NestedQuery
                {
                    Path = ExtractNestedPath(fullyQualifiedFieldName),
                    Query = query
                };
            }

            return query;
        }

        private static QueryContainer TranslateInNode(InNode node)
        {
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(node.Left);

            if (node.Right is not CollectionConstantNode collectionNode)
            {
                throw new NotImplementedException("Right node is not CollectionConstantNode!");
            }

            var values = new List<object>();

            foreach (var item in collectionNode.Collection)
            {
                values.Add(item.Value);
            }

            var query = new TermsQuery { Field = ExtractFieldName(fullyQualifiedFieldName), Terms = values };

            if (node.Left is SingleValuePropertyAccessNode singleValueNode && IsNavigationNode(singleValueNode.Source.Kind))
            {
                return new NestedQuery
                {
                    Path = ExtractNestedPath(fullyQualifiedFieldName),
                    Query = query,
                };
            }

            return query;
        }

        private static QueryContainer TranslateOperatorNode(BinaryOperatorNode node, ODataExpressionContext context = null)
        {
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(node.Left, context?.PathPrefix);
            var fieldName = ExtractFieldName(fullyQualifiedFieldName);

            var query = node.OperatorKind switch
            {
                BinaryOperatorKind.And => AggregateAndOperations(node),
                BinaryOperatorKind.Or => AggregateOrOperations(node),
                BinaryOperatorKind.Equal => new TermQuery { Field = fieldName, Value = ExtractValue(node.Right) },
                BinaryOperatorKind.NotEqual => !new TermQuery { Field = fieldName, Value = ExtractValue(node.Right) },
                BinaryOperatorKind.GreaterThan => new TermRangeQuery { Field = fieldName, GreaterThan = ExtractStringValue(node.Right) },
                BinaryOperatorKind.GreaterThanOrEqual => new TermRangeQuery { Field = fieldName, GreaterThanOrEqualTo = ExtractStringValue(node.Right) },
                BinaryOperatorKind.LessThan => new TermRangeQuery { Field = fieldName, LessThan = ExtractStringValue(node.Right) },
                BinaryOperatorKind.LessThanOrEqual => new TermRangeQuery { Field = fieldName, LessThanOrEqualTo = ExtractStringValue(node.Right) },
                _ => throw new NotImplementedException($"Unsupported binary operator: {node.OperatorKind}"),
            };

            if (node.Left is SingleValuePropertyAccessNode singleValueNode && IsNavigationNode(singleValueNode.Source.Kind))
            {
                return new NestedQuery
                {
                    Path = ExtractNestedPath(fullyQualifiedFieldName),
                    Query = query,
                };
            }

            return query;
        }

        private static QueryContainer TranslateFunctionCallNode(SingleValueFunctionCallNode node, ODataExpressionContext context = null)
        {
            var left = node.Parameters.First();
            var right = node.Parameters.Last();
            var fullyQualifiedFieldName = ExtractFullyQualifiedFieldName(left, context?.PathPrefix);
            var fieldName = ExtractFieldName(fullyQualifiedFieldName);
            var value = ExtractValue(right);

            var query = node.Name.ToLower() switch
            {
                "startswith" => (QueryContainer)new PrefixQuery { Field = fieldName, Value = value },
                "endswith" => new WildcardQuery { Field = fieldName, Value = $"*{value}" },
                "contains" => new WildcardQuery { Field = fieldName, Value = $"*{value}*" },
                "substringof" => new MatchQuery { Field = fieldName, Query = value.ToString() },
                _ => throw new NotImplementedException($"Unsupported function: {node.Name}"),
            };

            if (left is SingleValuePropertyAccessNode singleValueNode && IsNavigationNode(singleValueNode.Source.Kind))
            {
                return new NestedQuery
                {
                    Path = ExtractNestedPath(fullyQualifiedFieldName),
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

        private static bool IsNavigationNode(QueryNodeKind kind)
        {
            return kind == QueryNodeKind.SingleNavigationNode ||
                kind == QueryNodeKind.CollectionNavigationNode;
        }

        private static string ExtractFullyQualifiedFieldName(QueryNode node, string prefix = null)
        {
            var segments = new List<string>();

            void ProcessNode(QueryNode currentNode)
            {
                switch (currentNode)
                {
                    case SingleValuePropertyAccessNode singleValue:
                        segments.Insert(0, singleValue.Property.Name);
                        ProcessNode(singleValue.Source);
                        break;
                    case SingleNavigationNode singleNavigationNode:
                        segments.Insert(0, singleNavigationNode.NavigationProperty.Name);
                        ProcessNode(singleNavigationNode.Source);
                        break;
                    case CollectionPropertyAccessNode collectionNode:
                        segments.Insert(0, collectionNode.Property.Name);
                        ProcessNode(collectionNode.Source);
                        break;
                    case CollectionNavigationNode collectionNavigationNode:
                        segments.Insert(0, collectionNavigationNode.NavigationProperty.Name);
                        ProcessNode(collectionNavigationNode.Source);
                        break;
                }
            }

            ProcessNode(node);

            if (segments.Count == 0)
            {
                return prefix;
            }

            if (prefix != null)
            {
                segments.Insert(0, prefix);
            }

            return string.Join(".", segments);
        }

        private static string ExtractNestedPath(string fullyQualifiedFieldName)
        {
            if (fullyQualifiedFieldName == null)
            {
                return null;
            }

            var lastIndex = fullyQualifiedFieldName.LastIndexOf('.');

            return lastIndex > 0 ? fullyQualifiedFieldName[..lastIndex] : fullyQualifiedFieldName;
        }

        private static string ExtractFieldName(string fullyQualifiedFieldName)
        {
            if (fullyQualifiedFieldName == null)
            {
                return null;
            }

            var lastIndex = fullyQualifiedFieldName.LastIndexOf('.') + 1;

            return lastIndex > 1 ? fullyQualifiedFieldName[lastIndex..] : fullyQualifiedFieldName;
        }

        private static object ExtractValue(QueryNode node)
        {
            if (node is ConstantNode constantNode)
            {
                if (constantNode.TypeReference is IEdmEnumTypeReference)
                {
                    return constantNode.Value?.ToString();
                }

                return constantNode.Value;
            }

            throw new NotImplementedException("Complex values are not supported yet.");
        }

        private static string ExtractStringValue(QueryNode node)
        {
            if (node is ConstantNode constantNode)
            {
                return constantNode.Value?.ToString();
            }

            throw new NotImplementedException("Complex values are not supported yet.");
        }

        internal class ODataExpressionContext
        {
            public string PathPrefix { get; set; }
        }
    }
}
