using Nest.OData.Tests.Common;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.OData.Tests
{
    public class ODataQueryOptionsTests
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

            var queryContainer = queryOptions.ToQueryContainer();

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
        public void EqOperatorWithEnumAsStringProperty()
        {
            var queryOptions = Mock.GetODataQueryOptions<Product>("$filter=Color eq 'Red'");

            var queryContainer = queryOptions.ToQueryContainer();

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

            var queryContainer = queryOptions.ToQueryContainer();

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

            var queryContainer = queryOptions.ToQueryContainer();

            Assert.NotNull(queryContainer);

            var queryJson = queryContainer.ToJson();

            var expectedJson = @"
            {""query"":{""bool"":{""minimum_should_match"":1,""should"":[{""term"":{""Category"":{""value"":""Goods""}}},{""term"":{""Color"":{""value"":""Red""}}}]}}}";

            var actualJObject = JObject.Parse(queryJson);
            var expectedJObject = JObject.Parse(expectedJson);

            // Assert
            Assert.True(JToken.DeepEquals(expectedJObject, actualJObject), "Expected and actual JSON do not match.");
        }
    }
}
