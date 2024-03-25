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
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Category eq 'Goods' and Color eq 'Red'");

            Assert.NotNull(queryOptions);
            Assert.NotNull(queryOptions.Filter);
            Assert.Equal("Category eq 'Goods' and Color eq 'Red'", queryOptions.Filter.RawValue);
        }

        [Fact]
        public void EqOperatorWithStringProperty()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Category eq 'Goods'");

            Assert.Equal("Category eq 'Goods'", queryOptions.Filter.RawValue);

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

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

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void InOperator()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Category in ('Milk', 'Cheese')");

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

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

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void EqOperatorWithEnumAsStringProperty()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Color eq 'Red'");

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {""query"":{""term"":{""Color"":{""value"":""Red""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void AndOperator()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Category eq 'Goods' and Color eq 'Red'");

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {""query"":{""bool"":{""must"":[{""term"":{""Category"":{""value"":""Goods""}}},{""term"":{""Color"":{""value"":""Red""}}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void OrOperator()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Category eq 'Goods' or Color eq 'Red'");

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {""query"":{""bool"":{""minimum_should_match"":1,""should"":[{""term"":{""Category"":{""value"":""Goods""}}},{""term"":{""Color"":{""value"":""Red""}}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void OrMultipleOperators()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Id eq 40938 and ((Color eq 'Red') or (Color eq 'Green') or (Color eq 'Blue'))");

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

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

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void AndMultipleOperators()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Id eq 69 or ((Color eq 'Red') and (Category eq 'Goods') and (Name eq 'Phone'))");

            var queryContainer = queryOptions.ToElasticsearchQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

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

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
