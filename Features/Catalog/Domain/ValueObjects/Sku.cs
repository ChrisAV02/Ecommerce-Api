using System.Text.RegularExpressions;
using Ecommerce_Api.Features.Catalog.Domain.Exceptions;

namespace Ecommerce_Api.Features.Catalog.Domain.ValueObjects;

public sealed record Sku
{
    private static readonly Regex ValidFormat = new("^[A-Z0-9][A-Z0-9._-]{0,49}$", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private Sku() { }

    private Sku(string value) => Value = value;

    public string Value { get; private set; } = string.Empty;

    public static Sku Create(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !ValidFormat.IsMatch(normalized))
        {
            throw new CatalogDomainException("catalog.sku.invalid", "SKU is required and may contain only letters, digits, dots, underscores, and hyphens (maximum 50 characters).");
        }

        return new Sku(normalized);
    }

    public override string ToString() => Value;
}
