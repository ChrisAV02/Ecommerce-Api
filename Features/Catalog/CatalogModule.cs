using Ecommerce_Api.Features.Catalog.Application.Contracts;
using Ecommerce_Api.Features.Catalog.Application.Services;
using Ecommerce_Api.Features.Catalog.Application.Services.Impl;
using Ecommerce_Api.Features.Catalog.Domain.Repositories;
using Ecommerce_Api.Features.Catalog.Infrastructure.Integrations;
using Ecommerce_Api.Features.Catalog.Infrastructure.Repositories.Impl;
using Ecommerce_Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace Ecommerce_Api.Features.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOpenApi(options => options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            if (operation.OperationId != "Catalog_GetProducts") return Task.CompletedTask;
            foreach (var parameter in operation.Parameters ?? [])
            {
                parameter.Description = parameter.Name == "page"
                    ? "Número de página, empezando en 1."
                    : "Cantidad de productos por página, entre 1 y 100.";
            }
            operation.Responses!["200"].Description = "Consulta exitosa. Una página vacía devuelve items: [].";
            operation.Responses["400"].Description = "Parámetros de paginación inválidos.";
            operation.Responses["500"].Description = "Error inesperado.";
            operation.Responses["200"].Content!["application/json"].Example = JsonNode.Parse("""
                {"items":[{"productId":"1c912343-48e2-420b-852f-cd9374a19f40","sku":"DEMO-001","name":"Arroz Extra 1 kg (DEMO)","description":"Ejemplo ficticio de contrato; no confirma stock.","basePrice":{"amount":4.9,"currency":"PEN"},"isActive":true}],"page":1,"pageSize":10,"totalCount":1}
                """);
            operation.Responses["400"].Content!["application/problem+json"].Example = JsonNode.Parse("""
                {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"page":["Debe ser mayor o igual a 1."]}}
                """);
            operation.Responses["500"].Content!["application/problem+json"].Example = JsonNode.Parse("""
                {"type":"https://tools.ietf.org/html/rfc9110#section-15.6.1","title":"An error occurred while processing your request.","status":500}
                """);
            return Task.CompletedTask;
        }));
        var connectionString = configuration.GetPostgreSqlConnectionString();
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres => postgres.MigrationsHistoryTable("__ef_migrations_history_catalog", "catalog")));

        var source = configuration["Catalog:DataSource"] ?? "PostgreSQL";
        if (source is not ("PostgreSQL" or "Demo"))
            throw new InvalidOperationException("Catalog:DataSource must be PostgreSQL or Demo.");
        if (source == "Demo" && !environment.IsDevelopment())
            throw new InvalidOperationException("Catalog Demo is only available in Development.");
        services.AddScoped<IProductRepository>(provider =>
        {
            if (source == "Demo")
            {
                return new InMemoryProductRepository();
            }
            return new ProductRepository(provider.GetRequiredService<CatalogDbContext>());
        });
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICatalogBrowsingRepository, CatalogBrowsingRepository>();
        services.AddScoped<ICatalogBrowsingService, CatalogBrowsingService>();
        services.AddHttpClient<IPosCatalogSource, PosCatalogSource>("Catalog.PosSynchronization", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue<int?>("Pos:TimeoutSeconds") ?? 15, 1, 60));
        });
        services.AddScoped<ICatalogSynchronizationStore, CatalogSynchronizationRepository>();
        services.AddScoped<ICatalogSynchronizationService, CatalogSynchronizationService>();
        services.AddScoped<Ecommerce_Api.Shared.Application.Contracts.IOrderQuoteProvider, CatalogOrderQuoteProvider>();
        return services;
    }
}
