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
    }
}
