using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Nest.OData.Tests.Common;

namespace Nest.OData.Sample.Controllers
{
    public class ProductsController : ODataController
    {
        private readonly IList<Product> products;
        private static readonly string[] value = ["Leonard G. Lobel", "Eric D. Boyd"];

        public ProductsController()
        {
            products = [];

            for (int i = 1; i < 30; i++)
            {
                var prod = new Product()
                {
                    Id = i,
                    Category = "Goods" + i,
                    Color = Color.Red,
                    Others = ["Others1", "Others2", "Others3"],
                    CreatedDate = new DateTimeOffset(2001, 4, 15, 16, 24, 8, TimeSpan.FromHours(-8)),
                    UpdatedDate = new DateTimeOffset(2011, 2, 15, 16, 24, 8, TimeSpan.FromHours(-8)),
                    ProductDetail = new ProductDetail { Id = "Id" + i, Info = "Info" + i },
                    ProductOrders = [
                        new Order
                        {
                            Id = i,
                            OrderNo = "Order"+i
                        }
                    ],
                    ProductSuppliers =
                    [
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
                    ],
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
        public IActionResult Get(ODataQueryOptions<Product> queryOptions)
        {
            var queryContainer = queryOptions.ToQueryContainer();

            return Ok(products);
        }
    }
}
