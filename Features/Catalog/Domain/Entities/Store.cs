using System.Text.RegularExpressions;
using Ecommerce_Api.Features.Catalog.Domain.Exceptions;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;

namespace Ecommerce_Api.Features.Catalog.Domain.Entities;

public sealed class Store
{
    private static readonly Regex ValidCode = new("^[A-Z0-9][A-Z0-9._-]{0,29}$", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private Store() { }

    private Store(Guid id, string code, string name, string address, GeoCoordinate? location, bool pickupEnabled, bool deliveryEnabled, DateTimeOffset createdAt)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = Product.NormalizeRequired(name, 200, "Store name");
        Address = Product.NormalizeRequired(address, 500, "Store address");
        Location = location;
        PickupEnabled = pickupEnabled;
        DeliveryEnabled = deliveryEnabled;
        IsActive = true;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public GeoCoordinate? Location { get; private set; }
    public bool PickupEnabled { get; private set; }
    public bool DeliveryEnabled { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Store Create(string code, string name, string address, GeoCoordinate? location, bool pickupEnabled, bool deliveryEnabled, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), code, name, address, location, pickupEnabled, deliveryEnabled, createdAt);

    public void UpdateInformation(string name, string address, bool pickupEnabled, bool deliveryEnabled, DateTimeOffset updatedAt)
    {
        Name = Product.NormalizeRequired(name, 200, "Store name");
        Address = Product.NormalizeRequired(address, 500, "Store address");
        PickupEnabled = pickupEnabled;
        DeliveryEnabled = deliveryEnabled;
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void UpdateLocation(GeoCoordinate? location, DateTimeOffset updatedAt)
    {
        Location = location;
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void EnablePickup(DateTimeOffset updatedAt) { PickupEnabled = true; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void DisablePickup(DateTimeOffset updatedAt) { PickupEnabled = false; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void EnableDelivery(DateTimeOffset updatedAt) { DeliveryEnabled = true; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void DisableDelivery(DateTimeOffset updatedAt) { DeliveryEnabled = false; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void Activate(DateTimeOffset updatedAt) { IsActive = true; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void Deactivate(DateTimeOffset updatedAt) { IsActive = false; UpdatedAt = updatedAt.ToUniversalTime(); }

    private static string NormalizeCode(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        return !string.IsNullOrWhiteSpace(normalized) && ValidCode.IsMatch(normalized)
            ? normalized
            : throw new CatalogDomainException("catalog.store.code.invalid", "Store code is required and must use up to 30 letters, digits, dots, underscores, or hyphens.");
    }
}
