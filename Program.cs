using Ecommerce_Api.Configurations;
using Ecommerce_Api.Features.Billing;
using Ecommerce_Api.Features.Catalog;
using Ecommerce_Api.Features.Incidents;
using Ecommerce_Api.Features.Inventory;
using Ecommerce_Api.Features.Payments;
using Ecommerce_Api.Features.Sales;
using Ecommerce_Api.Shared.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services
    .AddRedisConfiguration(builder.Configuration)
    .AddOpenApiConfiguration()
    .AddCorsConfiguration(builder.Configuration)
    .AddProblemDetailsConfiguration()
    .AddCatalogModule(builder.Configuration, builder.Environment)
    .AddInventoryModule()
    .AddSalesModule()
    .AddIncidentsModule()
    .AddPaymentsModule()
    .AddBillingModule();

var app = builder.Build();

app.MapOpenApiConfiguration();
app.UseProblemDetailsConfiguration();
app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("GetHealth")
    .WithTags("Health");

app.MapControllers();
app.MapInventoryEndpoints();
app.MapSalesEndpoints();
app.MapIncidentsEndpoints();
app.MapPaymentsEndpoints();
app.MapBillingEndpoints();

app.Run();
