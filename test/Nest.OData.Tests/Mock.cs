using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Nest.OData.Tests.Common;

namespace Nest.OData.Tests
{
    public static class Mock
    {
        public static IServiceProvider GetServiceProvider(this IEdmModel edmModel)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockODataFeature = new Mock<IODataFeature>();

            mockODataFeature.Setup(o => o.Model).Returns(edmModel);
            mockServiceProvider.Setup(s => s.GetService(typeof(IODataFeature)))
                .Returns(mockODataFeature.Object);

            return mockServiceProvider.Object;
        }

        public static ODataQueryOptions<T> GetODataQueryOptions<T>(string queryString)
        {
            var edmModel = EdmModelBuilder.GetEdmModel();
            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString($"?&{queryString}");
            context.RequestServices = edmModel.GetServiceProvider();

            return new ODataQueryOptions<T>(new ODataQueryContext(edmModel, typeof(T), new ODataPath()), context.Request);
        }

        public static string ToJson(this QueryContainer queryContainer)
        {
            var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")))
                .DefaultIndex("dummy");
            var elasticClient = new ElasticClient(settings);

            using var stream = new MemoryStream();
            elasticClient.RequestResponseSerializer.Serialize(new SearchRequest { Query = queryContainer }, stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
