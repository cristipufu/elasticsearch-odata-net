using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class ODataQueryOptionsNestedFieldTests
    {
        [Fact]
        public void NestedFieldContains()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=contains(ProductDetail/Info, 'searchTerm')");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""wildcard"": {
                  ""ProductDetail.Info"": {
                    ""value"": ""*searchTerm*""
                  }
                }
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedMultipleFieldsGt()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=ProductDetail/ProductRating/Rating gt 1");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""range"": {
                  ""ProductDetail.ProductRating.Rating"": {
                    ""gt"": ""1""
                  }
                }
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
