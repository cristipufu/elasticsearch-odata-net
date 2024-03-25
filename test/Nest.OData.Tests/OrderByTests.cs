using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class OrderByTests
    {
        [Fact]
        public void SkipTakeOrderByDesc()
        {
            var queryOptions = "$skip=10&$top=20&$orderby=CreatedDate desc".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""from"": 10,
              ""size"": 20,
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

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void MultipleOrderBy()
        {
            var queryOptions = "$orderby=CreatedDate desc,Category".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""sort"": [
                {""CreatedDate"": {""order"": ""desc""}},
                {""Category"": {""order"": ""asc""}}
              ]
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void OrderByNested()
        {
            var queryOptions = "$orderby=Product/Id desc".GetODataQueryOptions<Product>();

            var elasticQuery = queryOptions.ToElasticQuery();

            Assert.NotNull(elasticQuery);

            var queryJson = elasticQuery.ToJson();

            var expectedJson = @"
            {
              ""sort"": [
                {
                  ""Product.Id"": {
                    ""nested"": {
                      ""path"": ""Product""
                    },
                    ""order"": ""desc""
                  }
                }
              ]
            }";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
