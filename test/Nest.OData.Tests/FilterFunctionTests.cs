using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class FilterFunctionTests
    {
        [Fact]
        public void StartsWithFunction()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=startswith(Category, 'Goods')");

            var queryContainer = queryOptions.ToElasticQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
                ""query"": {
                    ""prefix"": {
                        ""Category"": {
                            ""value"": ""Goods""
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
        public void EndsWithFunction()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=endswith(Category, 'Goods')");

            var queryContainer = queryOptions.ToElasticQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
                ""query"": {
                    ""wildcard"": {
                        ""Category"": {
                            ""value"": ""*Goods""
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
        public void ContainsFunction()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=contains(Category, 'Goods')");

            var queryContainer = queryOptions.ToElasticQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""wildcard"": {
                  ""Category"": {
                    ""value"": ""*Goods*""
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
