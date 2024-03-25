using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class AggregationTests
    {
        [Fact]
        public void GroupBySimpleProperty()
        {
            var queryOptions = "$apply=groupby((Category))".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{""aggs"":{""group_by_Category"":{""terms"":{""field"":""Category""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void GroupByComplexProperty()
        {
            var queryOptions = "$apply=groupby((ProductDetail/Info))".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""aggs"": {
                ""nested_ProductDetail_Info"": {
                  ""nested"": {
                    ""path"": ""ProductDetail""
                  },
                  ""aggs"": {
                    ""group_by_ProductDetail_Info"": {
                      ""terms"": {
                        ""field"": ""ProductDetail.Info""
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
        public void GroupByComplexAndSimpleProperty()
        {
            var queryOptions = "$apply=groupby((ProductDetail/Info,Category))".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""aggs"": {
                ""group_by_Category"": {
                  ""terms"": {
                    ""field"": ""Category""
                  },
                  ""aggs"": {
                    ""nested_ProductDetail_Info"": {
                      ""nested"": {
                        ""path"": ""ProductDetail""
                      },
                      ""aggs"": {
                        ""group_by_ProductDetail_Info"": {
                          ""terms"": {
                            ""field"": ""ProductDetail.Info""
                          }
                        }
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
        public void GroupByMultipleSimpleProperties()
        {
            var queryOptions = "$apply=groupby((Category,Color))".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""aggs"": {
                ""group_by_Category"": {
                  ""terms"": {
                    ""field"": ""Category""
                  },
                  ""aggs"": {
                    ""group_by_Color"": {
                      ""terms"": {
                        ""field"": ""Color""
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
    }
}
