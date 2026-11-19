

namespace Ecommerce_Api.Features.Catalog.Domain.Entities;

public sealed class DeliveryZone
{
    private DeliveryZone() { }

    private DeliveryZone(Guid id, Guid storeId, string name, DateTimeOffset createdAt)
    {
        Id = id;
        StoreId = Product.RequireId(storeId, "Store");
        Name = Product.NormalizeRequired(name, 200, "Delivery zone name");
        IsActive = true;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static DeliveryZone Create(Guid storeId, string name, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), storeId, name, createdAt);

    public void UpdateInformation(string name, DateTimeOffset updatedAt)
    {
        Name = Product.NormalizeRequired(name, 200, "Delivery zone name");
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void Activate(DateTimeOffset updatedAt) { IsActive = true; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void Deactivate(DateTimeOffset updatedAt) { IsActive = false; UpdatedAt = updatedAt.ToUniversalTime(); }
}
