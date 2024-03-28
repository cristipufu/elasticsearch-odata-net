#if USE_ODATA_V7
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
#else
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
#endif
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

#nullable disable
namespace Nest.OData
{
    /// <summary>
    /// https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part2-url-conventions.html
    /// </summary>
    public static class ODataFilterExtensions
    {
        public static SearchDescriptor<T> Filter<T>(this SearchDescriptor<T> searchDescriptor, FilterQueryOption filter) where T : class
        {
            if (filter?.FilterClause?.Expression == null)
            {
                return searchDescriptor;
            }

            var queryContainer = TranslateExpression(filter.FilterClause.Expression);

            if (queryContainer == null)
            {
                return searchDescriptor.MatchAll();
            }

            return searchDescriptor.Query(q => queryContainer);
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
                MustNot =
                [
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

            var query = new TermsQuery { Field = fullyQualifiedFieldName, Terms = values };

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

            var query = node.OperatorKind switch
            {
                BinaryOperatorKind.And => TranslateAndOperations(node, context),
                BinaryOperatorKind.Or => TranslateOrOperations(node, context),
                BinaryOperatorKind.Equal => TranslateEqualOperation(node.Right, fullyQualifiedFieldName),
                BinaryOperatorKind.NotEqual => TranslateNotEqualOperation(node.Right, fullyQualifiedFieldName),
                BinaryOperatorKind.GreaterThan => new TermRangeQuery { Field = fullyQualifiedFieldName, GreaterThan = ExtractStringValue(node.Right) },
                BinaryOperatorKind.GreaterThanOrEqual => new TermRangeQuery { Field = fullyQualifiedFieldName, GreaterThanOrEqualTo = ExtractStringValue(node.Right) },
                BinaryOperatorKind.LessThan => new TermRangeQuery { Field = fullyQualifiedFieldName, LessThan = ExtractStringValue(node.Right) },
                BinaryOperatorKind.LessThanOrEqual => new TermRangeQuery { Field = fullyQualifiedFieldName, LessThanOrEqualTo = ExtractStringValue(node.Right) },
                _ => throw new NotImplementedException($"Unsupported binary operator: {node.OperatorKind}"),
            };

            if (ExtractSourceNode(node.Left) is SingleValuePropertyAccessNode singleValueNode && IsNavigationNode(singleValueNode.Source.Kind))
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
            var value = ExtractValue(right);

            var query = node.Name.ToLower() switch
            {
                "startswith" => (QueryContainer)new PrefixQuery { Field = fullyQualifiedFieldName, Value = value },
                "endswith" => new WildcardQuery { Field = fullyQualifiedFieldName, Value = $"*{value}" },
                "contains" => new WildcardQuery { Field = fullyQualifiedFieldName, Value = $"*{value}*" },
                "substringof" => new MatchQuery { Field = fullyQualifiedFieldName, Query = value.ToString() },
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

        private static QueryContainer TranslateOrOperations(BinaryOperatorNode node, ODataExpressionContext context = null)
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
                    queries.Add(TranslateExpression(queryNode, context));
                }
            }

            Collect(node);

            return new BoolQuery { Should = queries, MinimumShouldMatch = 1 };
        }

        private static QueryContainer TranslateAndOperations(BinaryOperatorNode node, ODataExpressionContext context = null)
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
                    queries.Add(TranslateExpression(queryNode, context));
                }
            }

            Collect(node);

            return new BoolQuery { Must = queries };
        }

        private static QueryContainer TranslateEqualOperation(SingleValueNode node, string fieldName)
        {
            var value = ExtractValue(node);

            if (value == null)
            {
                return !new ExistsQuery { Field = fieldName };
            }

            return new TermQuery { Field = fieldName, Value = value };
        }

        private static QueryContainer TranslateNotEqualOperation(SingleValueNode node, string fieldName)
        {
            var value = ExtractValue(node);

            if (value == null)
            {
                return new ExistsQuery { Field = fieldName };
            }

            return !new TermQuery { Field = fieldName, Value = value };
        }

        private static bool IsNavigationNode(QueryNodeKind kind)
        {
            return kind == QueryNodeKind.SingleNavigationNode ||
                kind == QueryNodeKind.CollectionNavigationNode;
        }

        private static SingleValueNode ExtractSourceNode(SingleValueNode node)
        {
            if (node is ConvertNode convertNode)
            {
                return convertNode.Source;
            }

            return node;
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
                    case ConvertNode convertNode:
                        ProcessNode(convertNode.Source);
                        break;
                    default:
                        break;
                }
            }

            ProcessNode(node);

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

        private static object ExtractValue(QueryNode node)
        {
            if (node is ConstantNode constantNode)
            {
                if (constantNode.TypeReference?.Definition?.TypeKind is EdmTypeKind.Enum)
                {
                    return (constantNode.Value as ODataEnumValue).Value?.ToString();
                }

                return constantNode.Value;
            }
            else if (node is ConvertNode convertNode)
            {
                return ExtractValue(convertNode.Source);
            }

            throw new NotImplementedException("Complex values are not supported yet.");
        }

        private static string ExtractStringValue(QueryNode node)
        {
            if (node is ConstantNode constantNode)
            {
                if (constantNode.Value is DateTime dateTime)
                {
                    return dateTime.ToString("o"); 
                }
                else if (constantNode.Value is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset.ToString("o");
                }
                else
                {
                    return constantNode.Value?.ToString();
                }
            }
            else if (node is ConvertNode convertNode)
            {
                return ExtractStringValue(convertNode.Source);
            }

            throw new NotImplementedException("Complex values are not supported yet.");
        }
    }
}
