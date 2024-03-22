using Microsoft.AspNetCore.OData;
using Nest.OData.Sample.SwashbuckleFilters;
using Nest.OData.Tests.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddOData(
    opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5).AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<ODataOperationFilter>();
    c.OperationFilter<RemoveODataQueryOptionsFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
