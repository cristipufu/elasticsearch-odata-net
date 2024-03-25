using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace Nest.OData
{
    public static class ODataExtensions
    {
        public static SearchDescriptor<T> ToElasticQuery<T>(this ODataQueryOptions<T> queryOptions) where T : class
        {
            return new SearchDescriptor<T>()
                .Query(queryOptions.Filter)
                .Aggregate(queryOptions.Apply)
                .OrderBy(queryOptions.OrderBy) 
                .Skip(queryOptions.Skip)
                .Take(queryOptions.Top);
        }

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
                    else if (node is OrderByOpenPropertyNode openPropertyNode)
                    {
                        if (openPropertyNode.OrderByClause.Expression is SingleValueOpenPropertyAccessNode singleValueNode)
                        {
                            if (singleValueNode.Source is SingleValueOpenPropertyAccessNode source)
                            {
                                s.Field(f => f.Field($"{source.Name}.{singleValueNode.Name}").Order(GetSortOrder(node.Direction)).Nested(n => n.Path(source.Name)));
                            }
                        }
                    }
                }

                return s;
            });

            return searchDescriptor;
        }

        public static SearchDescriptor<T> Skip<T>(this SearchDescriptor<T> searchDescriptor, SkipQueryOption skipQueryOption) where T : class
        {
            if (skipQueryOption == null)
            {
                return searchDescriptor;
            }

            return searchDescriptor.From(skipQueryOption.Value);
        }

        public static SearchDescriptor<T> Take<T>(this SearchDescriptor<T> searchDescriptor, TopQueryOption topQueryOption) where T : class
        {
            if (topQueryOption == null)
            {
                return searchDescriptor;
            }

            return searchDescriptor.Size(topQueryOption.Value);
        }
    }
}
