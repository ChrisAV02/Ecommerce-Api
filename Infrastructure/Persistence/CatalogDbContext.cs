using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<StoreAssortment> StoreAssortments => Set<StoreAssortment>();
    public DbSet<CatalogSynchronizationRun> SynchronizationRuns => Set<CatalogSynchronizationRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly, type =>
            type.Namespace?.StartsWith("Ecommerce_Api.Features.Catalog.Infrastructure.Configurations", StringComparison.Ordinal) == true);
        base.OnModelCreating(modelBuilder);
    }
}
