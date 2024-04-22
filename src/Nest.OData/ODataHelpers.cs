using Microsoft.OData.UriParser;

#nullable disable
namespace Nest.OData
{
    internal static class ODataHelpers
    {
        internal static string ExtractFullyQualifiedFieldName(QueryNode node, string prefix = null)
        {
            var segments = new List<string>();

            void ProcessNode(QueryNode currentNode)
            {
                switch (currentNode)
                {
                    case SingleValueFunctionCallNode singleValueFunction:
                        ProcessNode(singleValueFunction.Parameters.First());
                        break;
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
