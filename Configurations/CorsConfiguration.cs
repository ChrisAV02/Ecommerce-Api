namespace Ecommerce_Api.Configurations;

public static class CorsConfiguration
{
    public const string AllowedOriginsSetting = "Cors:AllowedOrigins";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection(AllowedOriginsSetting).Get<string[]>() ?? [];

        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        }));

        return services;
    }
}
