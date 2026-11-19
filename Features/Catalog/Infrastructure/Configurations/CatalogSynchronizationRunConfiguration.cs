using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Configurations;

public sealed class CatalogSynchronizationRunConfiguration : IEntityTypeConfiguration<CatalogSynchronizationRun>
{
    public void Configure(EntityTypeBuilder<CatalogSynchronizationRun> builder)
    {
        builder.ToTable("synchronization_run", "catalog");
        builder.HasKey(run => run.Id).HasName("pk_catalog_synchronization_run");
        builder.Property(run => run.Id).HasColumnName("synchronization_id").ValueGeneratedNever();
        builder.Property(run => run.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(run => run.StartedAt).HasColumnName("started_at");
        builder.Property(run => run.CompletedAt).HasColumnName("completed_at");
        builder.Property(run => run.SourceProducts).HasColumnName("source_products");
        builder.Property(run => run.SourceStores).HasColumnName("source_stores");
        builder.Property(run => run.SourceAssortments).HasColumnName("source_assortments");
        builder.Property(run => run.ProductsCreated).HasColumnName("products_created");
        builder.Property(run => run.ProductsUpdated).HasColumnName("products_updated");
        builder.Property(run => run.CategoriesCreated).HasColumnName("categories_created");
        builder.Property(run => run.StoresCreated).HasColumnName("stores_created");
        builder.Property(run => run.AssortmentsCreated).HasColumnName("assortments_created");
        builder.Property(run => run.MissingPriceSkusJson).HasColumnName("missing_price_skus_json").HasColumnType("jsonb");
        builder.Property(run => run.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
        builder.HasIndex(run => run.StartedAt).HasDatabaseName("ix_catalog_sync_started_at");
    }
}
