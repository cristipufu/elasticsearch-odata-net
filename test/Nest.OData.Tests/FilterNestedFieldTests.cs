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
            var queryOptions = "$filter=contains(ProductDetail/Info, 'searchTerm')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""wildcard"": {
                      ""ProductDetail.Info"": {
                        ""value"": ""*searchterm*""
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
        public void NestedMultipleFieldsGt()
        {
            var queryOptions = "$filter=ProductDetail/ProductRating/Rating gt 1".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail.ProductRating"",
                  ""query"": {
                    ""range"": {
                      ""ProductDetail.ProductRating.Rating"": {  
                        ""gt"": ""1""
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
        public void NestedAnyCollectionFieldsEq()
        {
            var queryOptions = "$filter=Tags/any(t: t eq 'Electronics')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {""query"":{""term"":{""Tags"":{""value"":""electronics""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeAnyCollectionFieldsEq()
        {
            var queryOptions = "$filter=ProductDetail/Tags/any(t: t eq 'Electronics')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""term"": {
                      ""ProductDetail.Tags"": {
                        ""value"": ""electronics""
                      }
                    }
                  }}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedLambdaComplexTypeEq()
        {
            var queryOptions = "$filter=ProductSuppliers/any(s: s/Name eq 'Electronics')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
                ""query"": {
                    ""nested"": {
                        ""path"": ""ProductSuppliers"",
                        ""query"": {
                        ""term"": {
                            ""ProductSuppliers.Name"": {
                            ""value"": ""electronics""
                            }
                        }
                    }
                }}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeInOperator()
        {
            var queryOptions = "$filter=ProductDetail/Id in (123, 456)".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""terms"": {
                      ""ProductDetail.Id"": [123, 456]
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
        public void NestedComplexTypeAllCollectionFieldsEq()
        {
            var queryOptions = "$filter=ProductDetail/Tags/all(t: t eq 'Electronics')".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

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
                                  ""ProductDetail.Tags"": {
                                    ""value"": ""electronics""
                                  }
                                }
                              }
                            ]
                          }
                        }]}}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeFieldEqualsNull()
        {
            var queryOptions = "$filter=ProductDetail/Id eq null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""bool"": {
                      ""must_not"": [
                        {
                          ""exists"": {
                            ""field"": ""ProductDetail.Id""
                          }
                        }
                      ]
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
        public void NestedComplexTypeFieldNotEqualsNull()
        {
            var queryOptions = "$filter=ProductDetail/Id ne null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail"",
                  ""query"": {
                    ""exists"": {
                      ""field"": ""ProductDetail.Id""
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
        public void NestedComplexTypeNotEqualsNull()
        {
            var queryOptions = "$filter=ProductDetail ne null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{""query"":{""exists"":{""field"":""ProductDetail""}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeEqualsNull()
        {
            var queryOptions = "$filter=ProductDetail eq null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{""query"":{""bool"":{""must_not"":[{""exists"":{""field"":""ProductDetail""}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void NestedComplexTypeNestedFieldNotEqualsNull()
        {
            var queryOptions = "$filter=ProductDetail/ProductRating/Id ne null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail.ProductRating"",
                  ""query"": {
                    ""exists"": {
                      ""field"": ""ProductDetail.ProductRating.Id""
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
        public void NestedComplexTypeNestedFieldEqualsNull()
        {
            var queryOptions = "$filter=ProductDetail/ProductRating/Id eq null".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"{
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail.ProductRating"",
                  ""query"": {
                    ""bool"": {
                      ""must_not"": [
                        {
                          ""exists"": {
                            ""field"": ""ProductDetail.ProductRating.Id""
                          }
                        }
                      ]
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
        public void NestedKeywordEqualsGuid()
        {
            var queryOptions = "$filter=ProductDetail/Key eq 12345678-1234-1234-1234-123456789abc".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductDetail.Key"",
                  ""query"": {
                    ""term"": {
                      ""ProductDetail.Key.keyword"": {
                        ""value"": ""12345678-1234-1234-1234-123456789abc""
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
        public void NestedCollectionKeywordEqualsGuid()
        {
            var queryOptions = "$filter=ProductOrders/any(s: s/Key eq 12345678-1234-1234-1234-123456789abc)".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""nested"": {
                  ""path"": ""ProductOrders"",
                  ""query"": {
                    ""term"": {
                      ""ProductOrders.Key.keyword"": {
                        ""value"": ""12345678-1234-1234-1234-123456789abc""
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
