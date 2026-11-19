using Ecommerce_Api.Features.Payments.Application.Services;
using Ecommerce_Api.Features.Payments.Application.Services.Impl;
using Ecommerce_Api.Features.Payments.Infrastructure.Repositories.Impl;
using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<PaymentReadSource>();
        services.AddScoped<IPaymentEvidence>(provider => provider.GetRequiredService<PaymentReadSource>());
        services.AddScoped<IRefundEvidence>(provider => provider.GetRequiredService<PaymentReadSource>());
        return services;
    }

    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map this module's HTTP endpoints here as they are introduced.
        return endpoints;
    }
}
