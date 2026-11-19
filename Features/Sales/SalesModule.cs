using Ecommerce_Api.Features.Sales.Application.Services;
using Ecommerce_Api.Features.Sales.Application.Services.Impl;
using Ecommerce_Api.Features.Sales.Infrastructure.Repositories.Impl;
using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Sales;

public static class SalesModule
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<OrderReadSource>();
        services.AddScoped<IOrderPaymentSource>(provider => provider.GetRequiredService<OrderReadSource>());
        services.AddScoped<IConfirmedOrderSource>(provider => provider.GetRequiredService<OrderReadSource>());
        services.AddScoped<IOrderCompensationSource>(provider => provider.GetRequiredService<OrderReadSource>());
        return services;
    }

    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map this module's HTTP endpoints here as they are introduced.
        return endpoints;
    }
}
