using Microsoft.AspNetCore.Mvc;
#if USE_ODATA_V7
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData;
#else
using Microsoft.AspNetCore.OData.Query;
#endif
using Nest.OData.Tests.Common;

namespace Nest.OData.Sample.Controllers
{
    public class ProductsController : ControllerBase
    {
        private readonly IList<Product> products;
        private static readonly string[] value = new string[] { "Leonard G. Lobel", "Eric D. Boyd" };

        public ProductsController()
        {
            products = new List<Product>();

            for (int i = 1; i < 30; i++)
            {
                var prod = new Product()
                {
                    Id = i,
                    Key = Guid.NewGuid(),
                    Category = "Goods" + i,
                    Color = Color.Red,
                    Tags = new List<string> { "Electronics", "Food", "Plants" },
                    CreatedDate = new DateTimeOffset(2001, 4, 15, 16, 24, 8, TimeSpan.FromHours(-8)),
                    UpdatedDate = new DateTimeOffset(2011, 2, 15, 16, 24, 8, TimeSpan.FromHours(-8)),
                    ProductDetail = new ProductDetail { Id = i, Info = "Info" + i },
                    ProductOrders = new List<Order>
                    {
                        new Order
                        {
                            Id = i,
                            OrderNo = "Order"+i
                        }
                    },
                    ProductSuppliers = new List<Supplier>
                    {
                        new Supplier
                        {
                            Id = i,
                            Name = "Supplier"+i,
                            Description = "SupplierDesc"+i,
                            SupplierAddress = new Location
                            {
                                City = "SupCity"+i,
                                Address = "SupAddre"+i
                            }
                        }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "Prop1", new DateTimeOffset(2014, 7, 3, 0, 0, 0, 0, new TimeSpan(0))},
                        { "Prop2", value },
                        { "Prop3", "Others"}
                    }
                };

                products.Add(prod);
            }
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IEnumerable<Product>> Get(ODataQueryOptions<Product> queryOptions)
        {
            queryOptions.ToElasticQuery();

            return Ok(products);
        }
    }
}
