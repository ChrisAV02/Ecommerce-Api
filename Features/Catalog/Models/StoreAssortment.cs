using Ecommerce_Api.Features.Catalog.Models;

namespace Ecommerce_Api.Features.Catalog.Models;

public sealed class StoreAssortment
{
    private StoreAssortment() { }

    private StoreAssortment(Guid id, Guid storeId, Guid productId, DateTimeOffset createdAt)
    {
        Id = id;
        StoreId = Product.RequireId(storeId, "Store");
        ProductId = Product.RequireId(productId, "Product");
        IsListed = true;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public bool IsListed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static StoreAssortment Create(Guid storeId, Guid productId, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), storeId, productId, createdAt);

    public void List(DateTimeOffset updatedAt) { IsListed = true; UpdatedAt = updatedAt.ToUniversalTime(); }
    public void Unlist(DateTimeOffset updatedAt) { IsListed = false; UpdatedAt = updatedAt.ToUniversalTime(); }
}
