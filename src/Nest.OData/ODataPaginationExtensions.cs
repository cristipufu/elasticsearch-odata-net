using Microsoft.AspNetCore.OData.Query;

namespace Nest.OData
{
    public static class ODataPaginationExtensions
    {
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
