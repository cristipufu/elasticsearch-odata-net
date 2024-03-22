using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

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

            var model = builder.GetEdmModel();
            model.MarkAsImmutable();

            return model;
        }
    }
}
