namespace Ecommerce_Api.Infrastructure.Caching;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string? ConnectionString { get; set; }
}

public static class RedisConfiguration
{
    public static IServiceCollection AddRedisConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        return services;
    }
}
