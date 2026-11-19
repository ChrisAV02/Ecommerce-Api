using Ecommerce_Api.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce_Api.Features.Catalog.Persistence.Configurations;

public sealed class StoreAssortmentConfiguration : IEntityTypeConfiguration<StoreAssortment>
{
    public void Configure(EntityTypeBuilder<StoreAssortment> builder)
    {
        builder.ToTable("store_assortment", "catalog");
        builder.HasKey(assortment => assortment.Id).HasName("pk_store_assortment");
        builder.Property(assortment => assortment.Id).HasColumnName("store_assortment_id").ValueGeneratedNever();
        builder.Property(assortment => assortment.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(assortment => assortment.ProductId).HasColumnName("product_id").IsRequired();
        builder.HasOne<Store>().WithMany().HasForeignKey(assortment => assortment.StoreId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_store_assortment_store");
        builder.HasOne<Product>().WithMany().HasForeignKey(assortment => assortment.ProductId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_store_assortment_product");
        builder.HasIndex(assortment => new { assortment.StoreId, assortment.ProductId })
            .IsUnique().HasDatabaseName("uq_store_assortment_store_id_product_id");
        builder.HasIndex(assortment => assortment.ProductId).HasDatabaseName("ix_store_assortment_product_id");
        builder.Property(assortment => assortment.IsListed).HasColumnName("is_listed").IsRequired();
        builder.Property(assortment => assortment.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(assortment => assortment.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
    }
}
