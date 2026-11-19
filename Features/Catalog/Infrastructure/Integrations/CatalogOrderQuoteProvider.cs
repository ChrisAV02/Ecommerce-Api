using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.Exceptions;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;
using Ecommerce_Api.Infrastructure.Persistence;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Integrations;

// In-process Catalog port: server-owned commercial prices only; never checks or reserves POS stock.
public sealed class CatalogOrderQuoteProvider(CatalogDbContext db) : IOrderQuoteProvider
{
    public async Task<OrderQuote> QuoteAsync(string storeCode, IReadOnlyList<QuoteLine> items, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storeCode) || items is null || items.Count is < 1 or > 100
            || items.Any(item => item is null || string.IsNullOrWhiteSpace(item.Sku) || item.Quantity is < 1 or > 1000000))
            throw new WorkflowException(400, "catalog_quote_invalid_items");
        string[] skus;
        try { skus = items.Select(item => Sku.Create(item.Sku).Value).ToArray(); }
        catch (CatalogDomainException) { throw new WorkflowException(400, "catalog_quote_invalid_sku"); }
        if (skus.Distinct(StringComparer.Ordinal).Count() != skus.Length) throw new WorkflowException(400, "catalog_quote_duplicate_sku");
        var code = storeCode.Trim().ToUpperInvariant();
        var store = await db.Stores.AsNoTracking().SingleOrDefaultAsync(store => store.Code == code, ct)
            ?? throw new WorkflowException(404, "catalog_store_not_found");
        if (!store.IsActive || !store.PickupEnabled) throw new WorkflowException(409, "catalog_store_unavailable");
        var listedProducts = await (from assortment in db.StoreAssortments.AsNoTracking()
                                    join product in db.Products.AsNoTracking() on assortment.ProductId equals product.Id
                                    where assortment.StoreId == store.Id && assortment.IsListed && product.IsActive
                                    select product).ToListAsync(ct);
        var products = listedProducts.ToDictionary(product => product.Sku.Value, StringComparer.Ordinal);
        var quoted = new List<QuotedLine>(items.Count);
        string? currency = null;
        decimal total = 0;
        for (var index = 0; index < skus.Length; index++)
        {
            if (!products.TryGetValue(skus[index], out var product)) throw new WorkflowException(409, "catalog_product_not_listed");
            currency ??= product.BasePrice.Currency;
            if (currency != product.BasePrice.Currency) throw new WorkflowException(409, "catalog_quote_currency_mismatch");
            var quantity = items[index].Quantity;
            total = checked(total + product.BasePrice.Amount * quantity);
            quoted.Add(new(product.Sku.Value, product.Name, quantity, product.BasePrice.Amount));
        }
        return new(store.Id, store.Code, quoted, total, currency!);
    }
}
