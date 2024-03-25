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
            var queryOptions = Mock.GetODataQueryOptions<Product>("$apply=groupby((Category))");

            var queryContainer = queryOptions.ToElasticQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"{""aggs"":{""group_by_Category"":{""terms"":{""field"":""Category""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }

        [Fact]
        public void GroupByMultipleSimpleProperties()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$apply=groupby((Category,Color))");

            var queryContainer = queryOptions.ToElasticQuery();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"{""aggs"":{""group_by_Category"":{""aggs"":{""group_by_Color"":{""terms"":{""field"":""Color""}}},""terms"":{""field"":""Category""}}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
