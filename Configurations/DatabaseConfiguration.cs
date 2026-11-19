namespace Ecommerce_Api.Configurations;

public static class DatabaseConfiguration
{
    public const string DatabaseName = "ecommerce_db";

    public static string GetPostgreSqlConnectionString(this IConfiguration configuration) =>
        configuration.GetConnectionString("PostgreSQL") ?? string.Empty;
}
