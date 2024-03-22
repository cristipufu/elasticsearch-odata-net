using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nest.OData.Sample.SwashbuckleFilters
{
    public class ODataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= [];

            var odataParams = new List<OpenApiParameter>
            {
                new()
                {
                    Name = "$filter",
                    In = ParameterLocation.Query,
                    Description = "Filters the results, based on a Boolean condition.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                },
                new()
                {
                    Name = "$skip",
                    In = ParameterLocation.Query,
                    Description = "Skips the first n results.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                },
                new()
                {
                    Name = "$take",
                    In = ParameterLocation.Query,
                    Description = "Returns only the first n results.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                },
                new()
                {
                    Name = "$orderby",
                    In = ParameterLocation.Query,
                    Description = "Sorts the results.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                },
                new()
                {
                    Name = "$expand",
                    In = ParameterLocation.Query,
                    Description = "Expands related entities inline.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                }
            };

            foreach (var param in odataParams)
            {
                operation.Parameters.Add(param);
            }
        }
    }
}
