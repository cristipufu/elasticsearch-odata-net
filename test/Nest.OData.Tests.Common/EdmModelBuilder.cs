using Microsoft.OData.Edm;
#if USE_ODATA_V7
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
#endif

namespace Nest.OData.Tests.Common
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Supplier>("Suppliers");
            builder.EntitySet<Order>("Orders");

            return builder.GetEdmModel();
        }
    }
}
