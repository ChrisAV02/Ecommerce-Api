using Ecommerce_Api.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce_Api.Features.Catalog.Persistence.Configurations;

public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("store", "catalog", table =>
        {
            table.HasCheckConstraint("ck_store_latitude", "latitude >= -90 AND latitude <= 90");
            table.HasCheckConstraint("ck_store_longitude", "longitude >= -180 AND longitude <= 180");
            table.HasCheckConstraint("ck_store_location_complete", "(latitude IS NULL) = (longitude IS NULL)");
            table.HasCheckConstraint("ck_store_code_format", "code ~ '^[A-Z0-9][A-Z0-9._-]{0,29}$'");
        });
        builder.HasKey(store => store.Id).HasName("pk_store");
        builder.Property(store => store.Id).HasColumnName("store_id").ValueGeneratedNever();
        builder.Property(store => store.Code).HasColumnName("code").HasMaxLength(30).IsRequired();
        builder.HasIndex(store => store.Code).IsUnique().HasDatabaseName("uq_store_code");
        builder.Property(store => store.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(store => store.Address).HasColumnName("address").HasMaxLength(500).IsRequired();
        builder.OwnsOne(store => store.Location, location =>
        {
            location.Property(coordinate => coordinate.Latitude).HasColumnName("latitude").HasPrecision(9, 6);
            location.Property(coordinate => coordinate.Longitude).HasColumnName("longitude").HasPrecision(9, 6);
        });
        builder.Navigation(store => store.Location).IsRequired(false);
        builder.Property(store => store.PickupEnabled).HasColumnName("pickup_enabled").IsRequired();
        builder.Property(store => store.DeliveryEnabled).HasColumnName("delivery_enabled").IsRequired();
        builder.Property(store => store.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(store => store.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(store => store.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");
    }
}
