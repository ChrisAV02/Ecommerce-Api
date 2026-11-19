using Ecommerce_Api.Features.Incidents.Application.Services;
using Ecommerce_Api.Features.Incidents.Application.Services.Impl;
using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Incidents;

public static class IncidentsModule
{
    public static IServiceCollection AddIncidentsModule(this IServiceCollection services)
    {
        services.AddScoped<IncidentService>();
        services.AddScoped<IIncidentService>(provider => provider.GetRequiredService<IncidentService>());
        services.AddScoped<IPickingIncidentWriter>(provider => provider.GetRequiredService<IncidentService>());
        return services;
    }

    public static IEndpointRouteBuilder MapIncidentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map this module's HTTP endpoints here as they are introduced.
        return endpoints;
    }
}
