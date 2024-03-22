using Nest.OData.Tests.Common;
using Xunit;

namespace Nest.OData.Tests
{
    public class ODataQueryOptionsTests
    {
        [Fact]
        public void CanCreateQueryOptionsFromQueryString()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Category eq 'Goods' and Color eq 'Red'");

            Assert.NotNull(queryOptions);
            Assert.NotNull(queryOptions.Filter);
            Assert.Equal("Category eq 'Goods' and Color eq 'Red'", queryOptions.Filter.RawValue);
        }
    }
}
