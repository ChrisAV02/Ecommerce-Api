using System.Globalization;
using Ecommerce_Api.Features.Catalog.Domain.Exceptions;

namespace Ecommerce_Api.Features.Catalog.Domain.ValueObjects;

public sealed record Money
{
    private static readonly HashSet<string> KnownIsoCurrencies = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(culture => new RegionInfo(culture.Name).ISOCurrencySymbol)
        .ToHashSet(StringComparer.Ordinal);

    private Money() { }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "PEN";

    public static Money Create(decimal amount, string? currency = "PEN")
    {
        var normalizedCurrency = currency?.Trim().ToUpperInvariant();
        if (amount < 0m || amount > 9_999_999_999.99m || decimal.Round(amount, 2) != amount)
        {
            throw new CatalogDomainException("catalog.price.invalid", "Price must be non-negative and have no more than two decimal places.");
        }

        if (normalizedCurrency is null || !KnownIsoCurrencies.Contains(normalizedCurrency))
        {
            throw new CatalogDomainException("catalog.currency.invalid", "Currency must be a recognized three-letter ISO currency code.");
        }

        return new Money(amount, normalizedCurrency);
    }
}
