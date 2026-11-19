using Ecommerce_Api.Configurations;
using Ecommerce_Api.Features.Billing;
using Ecommerce_Api.Infrastructure;
using Ecommerce_Api.Infrastructure.Caching;
using Ecommerce_Api.Features.Catalog;
using Ecommerce_Api.Features.Incidents;
using Ecommerce_Api.Features.Inventory;
using Ecommerce_Api.Features.Payments;
using Ecommerce_Api.Features.Sales;
using Ecommerce_Api.Shared.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});
builder.Services.AddControllers();

builder.Services
    .AddSharedInfrastructure()
    .AddRedisConfiguration(builder.Configuration)
    .AddOpenApiConfiguration()
    .AddCorsConfiguration(builder.Configuration)
    .AddProblemDetailsConfiguration()
    .AddCatalogModule(builder.Configuration, builder.Environment)
    .AddInventoryModule(builder.Configuration)
    .AddSalesModule()
    .AddIncidentsModule()
    .AddPaymentsModule()
    .AddBillingModule();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var incoming = context.Request.Headers["X-Correlation-Id"];
    if (incoming.Count == 1 && incoming[0] is string value &&
        System.Text.RegularExpressions.Regex.IsMatch(value, "^[A-Za-z0-9._-]{1,128}$"))
        context.TraceIdentifier = value;
    context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;
    using (app.Logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = context.TraceIdentifier }))
        await next(context);
});

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
