#if USE_ODATA_V7
using Microsoft.AspNet.OData.Query;
#else
using Microsoft.AspNetCore.OData.Query;
#endif
using Microsoft.OData.UriParser;

namespace Nest.OData
{
    public static class ODataOrderByExtensions
    {
        public static SearchDescriptor<T> OrderBy<T>(this SearchDescriptor<T> searchDescriptor, OrderByQueryOption orderByQueryOption) where T : class
        {
            if (orderByQueryOption?.OrderByNodes == null || !orderByQueryOption.OrderByNodes.Any())
            {
                return searchDescriptor;
            }

            static SortOrder GetSortOrder(OrderByDirection direction)
            {
                return direction == OrderByDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
            }

            searchDescriptor.Sort(s =>
            {
                var orderByClause = orderByQueryOption.OrderByClause;

                while (orderByClause != null)
                {
                    var expressionNode = orderByClause.Expression;
                    var direction = orderByClause.Direction;
                    var fullyQualifiedFieldName = ODataHelpers.ExtractFullyQualifiedFieldName(expressionNode);

                    if (expressionNode is SingleValuePropertyAccessNode singleValueNode)
                    {
                        var propertyName = singleValueNode.Property.Name;

                        if (ODataHelpers.IsNavigationNode(singleValueNode.Source.Kind))
                        {
                            s.Field(f => f.Field(fullyQualifiedFieldName)
                            .Order(GetSortOrder(direction))
                            .Nested(n => n.Path(ODataHelpers.ExtractNestedPath(fullyQualifiedFieldName))));
                        }
                        else
                        {
                            s.Field(f => f.Field(propertyName).Order(GetSortOrder(direction)));
                        }
                    }

                    orderByClause = orderByClause.ThenBy;
                }

                return s;
            });

            return searchDescriptor;
        }
    }
}
