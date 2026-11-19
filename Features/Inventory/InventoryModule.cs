using Ecommerce_Api.Features.Inventory.Application.Contracts;
using Ecommerce_Api.Features.Inventory.Application.Services;
using Ecommerce_Api.Features.Inventory.Application.Services.Impl;
using Ecommerce_Api.Features.Inventory.Infrastructure.Integrations;

namespace Ecommerce_Api.Features.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IPosInventoryGateway, PosInventoryGateway>(client =>
        {
            var url = configuration["Pos:BaseUrl"];
            if (string.IsNullOrWhiteSpace(url)) return;
            client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddHttpClient<IPosReservationGateway, PosReservationGateway>(client =>
        {
            var url = configuration["Pos:BaseUrl"];
            if (string.IsNullOrWhiteSpace(url)) return;
            client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddScoped<IInventoryService, InventoryService>();
        return services;
    }

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map this module's HTTP endpoints here as they are introduced.
        return endpoints;
    }
}
