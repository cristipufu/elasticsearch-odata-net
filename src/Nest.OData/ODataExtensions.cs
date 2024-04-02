#if USE_ODATA_V7
using Microsoft.AspNet.OData.Query;
#else
using Microsoft.AspNetCore.OData.Query;
#endif

namespace Nest.OData
{
    public static class ODataExtensions
    {
        public static SearchDescriptor<T> ToElasticQuery<T>(this ODataQueryOptions<T> queryOptions) where T : class
        {
            return new SearchDescriptor<T>()
                .Filter(queryOptions.Filter)
                .SelectExpand(queryOptions.SelectExpand)
                .Apply(queryOptions.Apply)
                .OrderBy(queryOptions.OrderBy) 
                .Skip(queryOptions.Skip)
                .Top(queryOptions.Top);
        }
    }
}
