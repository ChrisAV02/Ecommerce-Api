using Ecommerce_Api.Infrastructure.Integrations;
using Ecommerce_Api.Infrastructure.Persistence;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Presentation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ecommerce_Api.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.TryAddScoped<IWorkflowStore, WorkflowStore>();
        services.TryAddScoped<WorkflowExceptionFilter>();
        services.AddHttpClient<IPosReservationClient, PosReservationClient>(client =>
            client.Timeout = TimeSpan.FromSeconds(5));
        return services;
    }
}
