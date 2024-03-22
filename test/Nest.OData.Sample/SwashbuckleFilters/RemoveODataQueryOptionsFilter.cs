using Microsoft.AspNetCore.OData.Query;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nest.OData.Sample.SwashbuckleFilters
{
    public class RemoveODataQueryOptionsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var odataQueryOptionsParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Type != null &&
                            p.Type.IsGenericType &&
                            p.Type.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>))
                .Select(p => p.Name)
                .ToList();

            foreach (var paramName in odataQueryOptionsParameters)
            {
                var param = operation.Parameters.FirstOrDefault(p => p.Name == paramName);
                if (param != null)
                {
                    operation.Parameters.Remove(param);
                }
            }
        }
    }
}
