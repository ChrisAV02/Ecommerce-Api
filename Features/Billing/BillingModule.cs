using Ecommerce_Api.Features.Billing.Application.Services;
using Ecommerce_Api.Features.Billing.Application.Services.Impl;

namespace Ecommerce_Api.Features.Billing;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services)
    {
        services.AddScoped<IBillingService, BillingService>();
        return services;
    }

    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map this module's HTTP endpoints here as they are introduced.
        return endpoints;
    }
}
