using Microsoft.AspNetCore.OData.Query;

namespace Nest.OData
{
    public static class ODataExtensions
    {
        public static SearchDescriptor<T> ToElasticsearchQuery<T>(this ODataQueryOptions<T> queryOptions) where T : class
        {
            var baseQuery = queryOptions.ToQueryContainer();

            var searchDescriptor = new SearchDescriptor<T>();

            if (baseQuery != null)
            {
                searchDescriptor.Query(q => baseQuery);
            }
            else
            {
                searchDescriptor.MatchAll();
            }

            return searchDescriptor.ApplyTransformations(queryOptions.Apply);
        }
    }
}
