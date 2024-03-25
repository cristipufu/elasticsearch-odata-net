using Microsoft.AspNetCore.OData.Query;

namespace Nest.OData.Tests
{
    internal static class Helpers
    {
        public static QueryContainer? ToQueryContainer<T>(this ODataQueryOptions<T> queryOptions)
        {
            if (queryOptions?.Filter?.FilterClause?.Expression == null)
            {
                return null;
            }

            return ODataFilterExtensions.TranslateExpression(queryOptions.Filter.FilterClause.Expression);
        }
    }
}
