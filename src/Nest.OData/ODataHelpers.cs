using Microsoft.OData.UriParser;
using System.Reflection;

#nullable disable
namespace Nest.OData
{
    internal static class ODataHelpers
    {
        internal static string ExtractFullyQualifiedFieldName(QueryNode node, ODataExpressionContext context = null)
        {
            var segments = new List<string>();

            void ProcessNode(QueryNode currentNode, Type currentType = null)
            {
                switch (currentNode)
                {
                    case SingleValueFunctionCallNode singleValueFunction:
                        ProcessNode(singleValueFunction.Parameters.First(), currentType);
                        break;
                    case SingleValuePropertyAccessNode singleValue:
                        var propertyName = singleValue.Property.Name;
                        var keywordAttribute = currentType?.GetProperty(propertyName)?.GetCustomAttribute<KeywordAttribute>();
                        if (keywordAttribute != null)
                        {
                            propertyName += ".keyword";
                        }
                        segments.Insert(0, propertyName);
                        currentType = UpdateCurrentType(currentType, singleValue.Property.Name);
                        ProcessNode(singleValue.Source, currentType);
                        break;
                    case SingleNavigationNode singleNavigationNode:
                        segments.Insert(0, singleNavigationNode.NavigationProperty.Name);
                        currentType = UpdateCurrentType(currentType, singleNavigationNode.NavigationProperty.Name);
                        ProcessNode(singleNavigationNode.Source, currentType);
                        break;
                    case CollectionPropertyAccessNode collectionNode:
                        segments.Insert(0, collectionNode.Property.Name);
                        currentType = UpdateCurrentType(currentType, collectionNode.Property.Name);
                        ProcessNode(collectionNode.Source, currentType);
                        break;
                    case CollectionNavigationNode collectionNavigationNode:
                        segments.Insert(0, collectionNavigationNode.NavigationProperty.Name);
                        currentType = UpdateCurrentType(currentType, collectionNavigationNode.NavigationProperty.Name);
                        ProcessNode(collectionNavigationNode.Source, currentType);
                        break;
                    case ConvertNode convertNode:
                        ProcessNode(convertNode.Source, currentType);
                        break;
                    default:
                        break;
                }
            }

            Type UpdateCurrentType(Type currentType, string propertyName)
            {
                var propertyInfo = currentType?.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    return GetPropertyType(propertyInfo.PropertyType);
                }
                return currentType;
            }

            Type GetPropertyType(Type propertyType)
            {
                if (propertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(propertyType.GetGenericTypeDefinition()))
                {
                    return propertyType.GetGenericArguments()[0];
                }
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                {
                    return propertyType.GetElementType() ?? propertyType;
                }
                return propertyType;
            }

            ProcessNode(node, context?.Type);

            if (context?.PathPrefix != null)
            {
                segments.Insert(0, context.PathPrefix);
            }

            return string.Join(".", segments);
        }

        internal static string ExtractNestedPath(string fullyQualifiedFieldName)
        {
            if (fullyQualifiedFieldName == null)
            {
                return null;
            }

            var lastIndex = fullyQualifiedFieldName.LastIndexOf('.');

            return lastIndex > 0 ? fullyQualifiedFieldName[..lastIndex] : fullyQualifiedFieldName;
        }

        internal static bool IsNavigationNode(QueryNodeKind kind)
        {
            return kind == QueryNodeKind.SingleNavigationNode ||
                kind == QueryNodeKind.CollectionNavigationNode;
        }
    }
}
