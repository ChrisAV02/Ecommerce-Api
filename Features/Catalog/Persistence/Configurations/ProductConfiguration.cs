using Ecommerce_Api.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce_Api.Features.Catalog.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("product", "catalog", table =>
        {
            table.HasCheckConstraint("ck_product_price_nonnegative", "base_price_amount >= 0");
            table.HasCheckConstraint("ck_product_base_price_currency", "base_price_currency ~ '^[A-Z]{3}$'");
            table.HasCheckConstraint("ck_product_sku_format", "sku ~ '^[A-Z0-9][A-Z0-9._-]{0,49}$'");
        });
        builder.HasKey(product => product.Id).HasName("pk_product");
        builder.Property(product => product.Id).HasColumnName("product_id").ValueGeneratedNever();
        builder.Property(product => product.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .HasConversion(sku => sku.Value, value => Sku.Create(value))
            .IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique().HasDatabaseName("uq_product_sku");
        builder.Property(product => product.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(product => product.CategoryId).HasColumnName("category_id").IsRequired();
        builder.HasOne<Category>().WithMany().HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_product_category");
        builder.OwnsOne(product => product.BasePrice, price =>
        {
            price.Property(money => money.Amount).HasColumnName("base_price_amount").HasPrecision(12, 2).IsRequired();
            price.Property(money => money.Currency).HasColumnName("base_price_currency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(product => product.BasePrice).IsRequired();
        builder.Property(product => product.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(product => product.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(product => product.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
        builder.HasIndex(product => product.CategoryId).HasDatabaseName("ix_product_category_id");
        builder.HasIndex(product => product.IsActive).HasDatabaseName("ix_product_is_active");
    }
}
