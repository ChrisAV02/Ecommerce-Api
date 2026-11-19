using Ecommerce_Api.Features.Catalog.Models;

namespace Ecommerce_Api.Features.Catalog.Models;

public sealed class Category
{
    private Category() { }

    private Category(Guid id, string name, string? description, DateTimeOffset createdAt)
    {
        Id = id;
        Name = Product.NormalizeRequired(name, 200, "Category name");
        Description = Product.NormalizeOptional(description, 1000, "Category description");
        IsActive = true;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Category Create(string name, string? description, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), name, description, createdAt);

    public void UpdateInformation(string name, string? description, DateTimeOffset updatedAt)
    {
        Name = Product.NormalizeRequired(name, 200, "Category name");
        Description = Product.NormalizeOptional(description, 1000, "Category description");
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        IsActive = true;
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt.ToUniversalTime();
    }
}
