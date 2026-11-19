using Ecommerce_Api.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce_Api.Features.Catalog.Persistence.Configurations;

public sealed class DeliveryZoneConfiguration : IEntityTypeConfiguration<DeliveryZone>
{
    public void Configure(EntityTypeBuilder<DeliveryZone> builder)
    {
        builder.ToTable("delivery_zone", "catalog");
        builder.HasKey(zone => zone.Id).HasName("pk_delivery_zone");
        builder.Property(zone => zone.Id).HasColumnName("delivery_zone_id").ValueGeneratedNever();
        builder.Property(zone => zone.StoreId).HasColumnName("store_id").IsRequired();
        builder.HasOne<Store>().WithMany().HasForeignKey(zone => zone.StoreId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_delivery_zone_store");
        builder.Property(zone => zone.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(zone => zone.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(zone => zone.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(zone => zone.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
        builder.HasIndex(zone => zone.StoreId).HasDatabaseName("ix_delivery_zone_store_id");
    }
}
