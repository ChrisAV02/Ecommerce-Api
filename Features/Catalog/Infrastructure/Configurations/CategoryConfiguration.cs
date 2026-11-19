using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category", "catalog");
        builder.HasKey(category => category.Id).HasName("pk_category");
        builder.Property(category => category.Id).HasColumnName("category_id").ValueGeneratedNever();
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(category => category.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(category => category.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(category => category.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(category => category.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
    }
}
