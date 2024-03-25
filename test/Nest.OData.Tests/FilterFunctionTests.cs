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
            var queryOptions = "$filter=startswith(Category, 'Goods')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

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

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void EndsWithFunction()
        {
            var queryOptions = "$filter=endswith(Category, 'Goods')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

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

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void ContainsFunction()
        {
            var queryOptions = "$filter=contains(Category, 'Goods')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

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

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
