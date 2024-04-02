#if USE_ODATA_V7
using Microsoft.AspNet.OData.Extensions;
#else
using Microsoft.AspNetCore.OData;
#endif
using Nest.OData.Sample.SwashbuckleFilters;
using Nest.OData.Tests.Common;

var builder = WebApplication.CreateBuilder(args);

#if USE_ODATA_V7
builder.Services.AddControllers();
builder.Services.AddOData().EnableApiVersioning();
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
});
builder.Services.AddODataApiExplorer(o => o.GroupNameFormat = "'v'VVVV");
builder.Services.AddEndpointsApiExplorer();
#else
builder.Services.AddControllers().AddOData(
    opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5).AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
); 
builder.Services.AddEndpointsApiExplorer();
#endif
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<ODataOperationFilter>();
    c.OperationFilter<RemoveODataQueryOptionsFilter>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

#if USE_ODATA_V7
app.UseRouting();
#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.Select().Filter().OrderBy().Count().MaxTop(10);
    endpoints.MapODataRoute("odata", "odata", EdmModelBuilder.GetEdmModel());
});
#pragma warning restore ASP0014 // Suggest using top level route registrations
#else
app.MapControllers();
#endif

app.Run();