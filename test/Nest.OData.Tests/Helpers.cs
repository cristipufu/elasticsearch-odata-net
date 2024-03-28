using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
#if USE_ODATA_V7
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#else
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
#endif
using Microsoft.OData.Edm;
using Nest.OData.Tests.Common;
using Moq;

namespace Nest.OData.Tests
{
    public static class Helpers
    {
        public static IServiceProvider GetServiceProvider(this IEdmModel edmModel)
        {
#if USE_ODATA_V7
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOData();
            services.AddSingleton(edmModel);
            var serviceProvider = services.BuildServiceProvider();
            var routeBuilder = new RouteBuilder(Mock.Of<IApplicationBuilder>(x => x.ApplicationServices == serviceProvider));
            routeBuilder.EnableDependencyInjection();
            return serviceProvider;
#else
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockODataFeature = new Mock<IODataFeature>();
            mockODataFeature.Setup(o => o.Model).Returns(edmModel);
            mockServiceProvider.Setup(s => s.GetService(typeof(IODataFeature)))
                .Returns(mockODataFeature.Object);
            return mockServiceProvider.Object;
#endif
        }

        public static ODataQueryOptions<T> GetODataQueryOptions<T>(this string queryString)
        {
            var edmModel = EdmModelBuilder.GetEdmModel();
            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString($"?&{queryString}");
            context.RequestServices = edmModel.GetServiceProvider();
#if USE_ODATA_V7
            return new ODataQueryOptions<T>(new ODataQueryContext(edmModel, typeof(T), new Microsoft.AspNet.OData.Routing.ODataPath()), context.Request);
#else
            return new ODataQueryOptions<T>(new ODataQueryContext(edmModel, typeof(T), new ODataPath()), context.Request);
#endif
        }

        public static string ToJson(this QueryContainer queryContainer)
        {
            var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")))
                .DefaultIndex("dummy");
            var client = new ElasticClient(settings);

            using var stream = new MemoryStream();
            client.RequestResponseSerializer.Serialize(new SearchRequest { Query = queryContainer }, stream);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string ToJson<T>(this SearchDescriptor<T> descriptor) where T : class
        {
            var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")))
                .DefaultIndex("dummy");
            var client = new ElasticClient(settings);

            using var stream = new MemoryStream();
            client.RequestResponseSerializer.Serialize(descriptor, stream);
            stream.Position = 0;
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
