namespace Ecommerce_Api.Shared.Presentation;

public static class ProblemDetailsConfiguration
{
    public static IServiceCollection AddProblemDetailsConfiguration(this IServiceCollection services)
    {
        services.AddProblemDetails();
        return services;
    }

    public static WebApplication UseProblemDetailsConfiguration(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
