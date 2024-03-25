﻿using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace Nest.OData
{
    public static class ODataExtensions
    {
        public static SearchDescriptor<T> ToElasticsearchQuery<T>(this ODataQueryOptions<T> queryOptions) where T : class
        {
            return new SearchDescriptor<T>()
                .ApplyFilter(queryOptions.Filter)
                .ApplyTransformations(queryOptions.Apply)
                .ApplyOrderBy(queryOptions.OrderBy) 
                .ApplySkip(queryOptions.Skip)
                .ApplyTop(queryOptions.Top);
        }

        public static SearchDescriptor<T> ApplyOrderBy<T>(this SearchDescriptor<T> searchDescriptor, OrderByQueryOption orderByQueryOption) where T : class
        {
            // todo check for complex properties or navigation properties

            if (orderByQueryOption == null || !orderByQueryOption.OrderByNodes.Any())
            {
                return searchDescriptor;
            }

            foreach (var node in orderByQueryOption.OrderByNodes)
            {
                if (node is OrderByPropertyNode propertyNode)
                {
                    var direction = propertyNode.Direction == OrderByDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
                    searchDescriptor.Sort(s => s.Field(f => f.Field(propertyNode.Property.Name).Order(direction)));
                }
            }

            return searchDescriptor;
        }

        public static SearchDescriptor<T> ApplySkip<T>(this SearchDescriptor<T> searchDescriptor, SkipQueryOption skipQueryOption) where T : class
        {
            if (skipQueryOption == null)
            {
                return searchDescriptor;
            }

            return searchDescriptor.From(skipQueryOption.Value);
        }

        public static SearchDescriptor<T> ApplyTop<T>(this SearchDescriptor<T> searchDescriptor, TopQueryOption topQueryOption) where T : class
        {
            if (topQueryOption == null)
            {
                return searchDescriptor;
            }

            return searchDescriptor.Size(topQueryOption.Value);
        }
    }
}
