using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class FilterOperatorTests
    {
        [Fact]
        public void QueryOptionsFromQueryString()
        {
            var queryOptions = "$filter=Category eq 'Goods' and Color eq 'Red'".GetODataQueryOptions<Product>();

            Assert.NotNull(queryOptions);
            Assert.NotNull(queryOptions.Filter);
            Assert.Equal("Category eq 'Goods' and Color eq 'Red'", queryOptions.Filter.RawValue);
        }

        [Fact]
        public void EqOperatorWithStringProperty()
        {
            var queryOptions = "$filter=Category eq 'Goods'".GetODataQueryOptions<Product>();

            Assert.Equal("Category eq 'Goods'", queryOptions.Filter.RawValue);

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
                ""query"": {
                    ""term"": {
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
        public void InOperator()
        {
            var queryOptions = "$filter=Category in ('Milk', 'Cheese')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""terms"": {
                  ""Category"": [""Milk"", ""Cheese""]
                }
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void EqOperatorWithEnumAsStringProperty()
        {
            var queryOptions = "$filter=Color eq 'Red'".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {""query"":{""term"":{""Color"":{""value"":""Red""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void AndOperator()
        {
            var queryOptions = "$filter=Category eq 'Goods' and Color eq 'Red'".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {""query"":{""bool"":{""must"":[{""term"":{""Category"":{""value"":""Goods""}}},{""term"":{""Color"":{""value"":""Red""}}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void OrOperator()
        {
            var queryOptions = "$filter=Category eq 'Goods' or Color eq 'Red'".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {""query"":{""bool"":{""minimum_should_match"":1,""should"":[{""term"":{""Category"":{""value"":""Goods""}}},{""term"":{""Color"":{""value"":""Red""}}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void OrMultipleOperators()
        {
            var queryOptions = "$filter=Id eq 40938 and ((Color eq 'Red') or (Color eq 'Green') or (Color eq 'Blue'))".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""term"": {
                        ""Id"": {
                          ""value"": 40938
                        }
                      }
                    },
                    {
                      ""bool"": {
                        ""minimum_should_match"": 1,
                        ""should"": [
                          {
                            ""term"": {
                              ""Color"": {
                                ""value"": ""Red""
                              }
                            }
                          },
                          {
                            ""term"": {
                              ""Color"": {
                                ""value"": ""Green""
                              }
                            }
                          },
                          {
                            ""term"": {
                              ""Color"": {
                                ""value"": ""Blue""
                              }
                            }
                          }
                        ]}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void AndMultipleOperators()
        {
            var queryOptions = "$filter=Id eq 69 or ((Color eq 'Red') and (Category eq 'Goods') and (Name eq 'Phone'))".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""minimum_should_match"": 1,
                  ""should"": [
                    {
                      ""term"": {
                        ""Id"": {
                          ""value"": 69
                        }
                      }
                    },
                    {
                      ""bool"": {
                        ""must"": [
                          {
                            ""term"": {
                              ""Color"": {
                                ""value"": ""Red""
                              }
                            }
                          },
                          {
                            ""term"": {
                              ""Category"": {
                                ""value"": ""Goods""
                              }
                            }
                          },
                          {
                            ""term"": {
                              ""Name"": {
                                ""value"": ""Phone""
                              }
                            }
                          }
                        ]}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void EqualsNull()
        {
            var queryOptions = "$filter=Category eq null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""must_not"": [
                    {
                      ""exists"": {
                        ""field"": ""Category""
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

        [Fact]
        public void EqualsNotNull()
        {
            var queryOptions = "$filter=Category ne null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""exists"": {
                  ""field"": ""Category""
                }
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void EqualsNullAndNotEqualsNull()
        {
            var queryOptions = "$filter=Category ne null and Color eq null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""exists"": {
                        ""field"": ""Category""
                      }
                    },
                    {
                      ""bool"": {
                        ""must_not"": [
                          {
                            ""exists"": {
                              ""field"": ""Color""
                            }
                          }
                        ]
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

        [Fact]
        public void EqualsGuid()
        {
            var queryOptions = "$filter=Key eq 12345678-1234-1234-1234-123456789abc".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""term"": {
                  ""Key"": {
                    ""value"": ""12345678-1234-1234-1234-123456789abc""
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
