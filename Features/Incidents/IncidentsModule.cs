namespace Ecommerce_Api.Features.Incidents;

public static class IncidentsModule
{
    public static IServiceCollection AddIncidentsModule(this IServiceCollection services)
    {
        // Register this module's application and infrastructure dependencies here as they are introduced.
        return services;
    }

    public static IEndpointRouteBuilder MapIncidentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Map this module's HTTP endpoints here as they are introduced.
        return endpoints;
    }
}
