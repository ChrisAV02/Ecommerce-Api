using Ecommerce_Api.Features.Catalog.Models;

namespace Ecommerce_Api.Features.Catalog.Models;

public sealed class Product
{
    private Product() { }

    private Product(Guid id, Sku sku, string name, string? description, Guid categoryId, Money basePrice, DateTimeOffset createdAt)
    {
        Id = id;
        Sku = sku;
        Name = NormalizeRequired(name, 200, "Product name");
        Description = NormalizeOptional(description, 1000, "Product description");
        CategoryId = RequireId(categoryId, "Category");
        BasePrice = basePrice ?? throw new CatalogDomainException("catalog.price.required", "Product price is required.");
        IsActive = true;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Money BasePrice { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Product Create(Sku sku, string name, string? description, Guid categoryId, Money basePrice, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), sku ?? throw new CatalogDomainException("catalog.sku.required", "Product SKU is required."), name, description, categoryId, basePrice, createdAt);

    public void UpdateInformation(string name, string? description, Guid categoryId, DateTimeOffset updatedAt)
    {
        Name = NormalizeRequired(name, 200, "Product name");
        Description = NormalizeOptional(description, 1000, "Product description");
        CategoryId = RequireId(categoryId, "Category");
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void ChangePrice(Money price, DateTimeOffset updatedAt)
    {
        BasePrice = price ?? throw new CatalogDomainException("catalog.price.required", "Product price is required.");
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

    internal static string NormalizeRequired(string? value, int maximumLength, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
        {
            throw new CatalogDomainException("catalog.field.invalid", $"{fieldName} is required and cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptional(string? value, int maximumLength, string fieldName)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized?.Length > maximumLength)
        {
            throw new CatalogDomainException("catalog.field.invalid", $"{fieldName} cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }

    internal static Guid RequireId(Guid id, string fieldName) => id == Guid.Empty
        ? throw new CatalogDomainException("catalog.identifier.invalid", $"{fieldName} identifier is required.")
        : id;
}
