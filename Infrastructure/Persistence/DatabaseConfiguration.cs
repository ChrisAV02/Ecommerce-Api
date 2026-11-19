namespace Ecommerce_Api.Infrastructure.Persistence;

public static class DatabaseConfiguration
{
    public const string DatabaseName = "ecommerce_db";

    public static string GetPostgreSqlConnectionString(this IConfiguration configuration) =>
        configuration.GetConnectionString("PostgreSQL") ?? string.Empty;
}
