using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class AdvancedQueryTests
    {
        [Fact]
        public void ExpandFilterByTimestampIncludeCount()
        {
            var queryOptions = "$expand=ProductFeature&$filter=CreatedDate gt 2024-01-01T00:00:00.00Z and CreatedDate lt 2025-01-01T00:00:00.00Z&$count=true"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""range"": {
                        ""CreatedDate"": {
                          ""gt"": ""2024-01-01T00:00:00.0000000+00:00""
                        }
                      }
                    },
                    {
                      ""range"": {
                        ""CreatedDate"": {
                          ""lt"": ""2025-01-01T00:00:00.0000000+00:00""
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

        [Fact]
        public void MultipleFiltersSkipTake()
        {
            var queryOptions = "$filter=Id eq 123 and (CreatedDate ge 2024-01-01T00:00:00.00Z and CreatedDate lt 2025-01-02T00:00:00.00Z) and Color ne 'Red'&$top=15&$skip=10"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""from"": 10,
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""term"": {
                        ""Id"": {
                          ""value"": 123
                        }
                      }
                    },
                    {
                      ""range"": {
                        ""CreatedDate"": {
                          ""gte"": ""2024-01-01T00:00:00.0000000+00:00""
                        }
                      }
                    },
                    {
                      ""range"": {
                        ""CreatedDate"": {
                          ""lt"": ""2025-01-02T00:00:00.0000000+00:00""
                        }
                      }
                    },
                    {
                      ""bool"": {
                        ""must_not"": [
                          {
                            ""term"": {
                              ""Color"": {
                                ""value"": ""Red""
                              }
                            }
                          }
                        ]
                      }
                    }
                  ]
                }
              },
              ""size"": 15
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void MultipleFiltersStartsWithNullComparison()
        {
            var queryOptions = "$filter=(startswith(Name, 'abc')) and (CreatedDate ne null) and (Color eq 'Green')"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""prefix"": {
                        ""Name"": {
                          ""value"": ""abc""
                        }
                      }
                    },
                    {
                      ""exists"": {
                        ""field"": ""CreatedDate""
                      }
                    },
                    {
                      ""term"": {
                        ""Color"": {
                          ""value"": ""Green""
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

        [Fact]
        public void MultipleExpandSelectMultipleSkipTop()
        {
            var queryOptions = "$expand=ProductDetail($expand=ProductRating)&$filter=Color eq 'Red'&$select=Id,Name,CreatedDate,Color&$top=100&$skip=10"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""from"": 10,
              ""query"": {
                ""term"": {
                  ""Color"": {
                    ""value"": ""Red""
                  }
                }
              },
              ""size"": 100,
              ""_source"": {
                ""includes"": [
                  ""Id"",
                  ""Name"",
                  ""CreatedDate"",
                  ""Color""
                ]
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void MultipleFiltersStartsWithSkipTopOrderBy()
        {
            var queryOptions = "$filter=Id eq 123 and ((Color eq 'Red') or (Color eq 'Green')) and startswith(Name,'abc')&$top=10&$skip=20&$orderby=CreatedDate desc"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""from"": 20,
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""term"": {
                        ""Id"": {
                          ""value"": 123
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
                          }
                        ]
                      }
                    },
                    {
                      ""prefix"": {
                        ""Name"": {
                          ""value"": ""abc""
                        }
                      }
                    }
                  ]
                }
              },
              ""size"": 10,
              ""sort"": [
                {
                  ""CreatedDate"": {
                    ""order"": ""desc""
                  }
                }
              ]
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void MultipleSelectMultipleFiltersMultipleOrderBy()
        {
            var queryOptions = "$select=Id,Name,Category&$filter=Id gt 123 and (Color eq 'Red') and startswith(Name,'abc')&$top=10&$skip=20&$orderby=CreatedDate desc, Category desc"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""from"": 20,
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""range"": {
                        ""Id"": {
                          ""gt"": ""123""
                        }
                      }
                    },
                    {
                      ""term"": {
                        ""Color"": {
                          ""value"": ""Red""
                        }
                      }
                    },
                    {
                      ""prefix"": {
                        ""Name"": {
                          ""value"": ""abc""
                        }
                      }
                    }
                  ]
                }
              },
              ""size"": 10,
              ""sort"": [
                {
                  ""CreatedDate"": {
                    ""order"": ""desc""
                  }
                },
                {
                  ""Category"": {
                    ""order"": ""desc""
                  }
                }
              ],
              ""_source"": {
                ""includes"": [
                  ""Id"",
                  ""Name"",
                  ""Category""
                ]
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void MultipleOrNestedContains()
        {
            var queryOptions = "$filter=((contains(ProductDetail/Info,'test')) or (contains(Category,'test')) or (contains(Name,'test')) or (contains(ProductFeature/Name,'test'))) and ((Color eq 'Red'))&$orderby=CreatedDate desc&$top=10"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""query"": {
                ""bool"": {
                  ""must"": [
                    {
                      ""bool"": {
                        ""minimum_should_match"": 1,
                        ""should"": [
                          {
                            ""nested"": {
                              ""path"": ""ProductDetail"",
                              ""query"": {
                                ""wildcard"": {
                                  ""ProductDetail.Info"": {
                                    ""value"": ""*test*""
                                  }
                                }
                              }
                            }
                          },
                          {
                            ""wildcard"": {
                              ""Category"": {
                                ""value"": ""*test*""
                              }
                            }
                          },
                          {
                            ""wildcard"": {
                              ""Name"": {
                                ""value"": ""*test*""
                              }
                            }
                          },
                          {
                            ""nested"": {
                              ""path"": ""ProductFeature"",
                              ""query"": {
                                ""wildcard"": {
                                  ""ProductFeature.Name"": {
                                    ""value"": ""*test*""
                                  }
                                }
                              }
                            }
                          }
                        ]
                      }
                    },
                    {
                      ""term"": {
                        ""Color"": {
                          ""value"": ""Red""
                        }
                      }
                    }
                  ]
                }
              },
              ""size"": 10,
              ""sort"": [
                {
                  ""CreatedDate"": {
                    ""order"": ""desc""
                  }
                }
              ]
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void ExpandMultipleSelectMultipleNestFilterSkipTop()
        {
            var queryOptions = "$expand=ProductDetail($expand=ProductRating)&$filter=(Color eq 'Red') and (Category eq 'Goods') and ProductDetail/Id eq 123&$select=Id,Name,Category&$top=5&$skip=5"
                .GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""from"": 5,
              ""query"": {
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
                          ""value"": ""goods""
                        }
                      }
                    },
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
                    }
                  ]
                }
              },
              ""size"": 5,
              ""_source"": {
                ""includes"": [
                  ""Id"",
                  ""Name"",
                  ""Category""
                ]
              }
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
