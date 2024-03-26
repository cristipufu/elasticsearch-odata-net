using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class ExpandTests
    {
        [Fact]
        public void ExpandWithFilter()
        {
            var queryOptions = "$expand=ProductDetail($filter=Id eq 123)".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""term"": {
                      ""ProductDetail.Id"": {
                        ""value"": 123
                      }
                    }
                  }
                }
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void ExpandMultipleWithFilter()
        {
            var queryOptions = "$expand=ProductDetail($filter=Id eq 123),ProductFeature($filter=Id eq 456)".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""nested"": {
                        ""path"": ""ProductDetail"",
                        ""query"": {
                          ""term"": {
                            ""ProductDetail.Id"": {
                              ""value"": 123
                            }
                          }
                        }
                      }
                    },
                    {
                      ""nested"": {
                        ""path"": ""ProductFeature"",
                        ""query"": {
                          ""term"": {
                            ""ProductFeature.Id"": {
                              ""value"": 456
                            }
                          }
                        }
                      }
                    }
                  ]
                }
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
