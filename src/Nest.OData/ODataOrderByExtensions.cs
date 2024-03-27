using Microsoft.AspNetCore.OData.Query;
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
                foreach (var node in orderByQueryOption.OrderByNodes)
                {
                    if (node is OrderByPropertyNode propertyNode)
                    {
                        s.Field(f => f.Field(propertyNode.Property.Name).Order(GetSortOrder(propertyNode.Direction)));
                    }
                    else if (node is OrderByOpenPropertyNode openPropertyNode &&
                        openPropertyNode.OrderByClause.Expression is SingleValueOpenPropertyAccessNode singleValueNode && 
                        singleValueNode.Source is SingleValueOpenPropertyAccessNode source)
                    {
                        s.Field(f => f.Field($"{source.Name}.{singleValueNode.Name}").Order(GetSortOrder(node.Direction)).Nested(n => n.Path(source.Name)));
                    }
                }

                return s;
            });

            return searchDescriptor;
        }
    }
}
