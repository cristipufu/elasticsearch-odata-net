using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class FilterNestedFieldTests
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
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""wildcard"": {
                      ""Info"": {
                        ""value"": ""*searchTerm*""
                      }
                    }
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
                ""nested"": {
                  ""path"": ""ProductDetail.ProductRating"",
                  ""query"": {
                    ""range"": {
                      ""Rating"": {  // Note the change here from the fully qualified path
                        ""gt"": ""1""
                      }
                    }
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
        public void NestedAnyCollectionFieldsEq()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Tags/any(t: t eq 'Electronics')");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {""query"":{""term"":{""Tags"":{""value"":""Electronics""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeAnyCollectionFieldsEq()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=ProductDetail/Tags/any(t: t eq 'Electronics')");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""term"": {
                      ""Tags"": {
                        ""value"": ""Electronics""
                      }
                    }
                  }}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedLambdaComplexTypeEq()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=ProductSuppliers/any(s: s/Name eq 'Electronics')");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
                ""query"": {
                    ""nested"": {
                        ""path"": ""ProductSuppliers"",
                        ""query"": {
                        ""term"": {
                            ""Name"": {
                            ""value"": ""Electronics""
                            }
                        }
                    }
                }}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeInOperator()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=ProductDetail/Id in (123, 456)");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""terms"": {
                      ""Id"": [123, 456]
                    }
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
        public void NestedComplexTypeAllCollectionFieldsEq()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=ProductDetail/Tags/all(t: t eq 'Electronics')");

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""bool"": {
                      ""must_not"": [
                        {
                          ""bool"": {
                            ""must_not"": [
                              {
                                ""term"": {
                                  ""Tags"": {
                                    ""value"": ""Electronics""
                                  }
                                }
                              }
                            ]
                          }
                        }]}}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
