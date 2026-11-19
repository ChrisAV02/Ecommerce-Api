using Ecommerce_Api.Features.Catalog.Domain.Repositories;
using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;
using Ecommerce_Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Repositories.Impl;

public sealed class CatalogBrowsingRepository(CatalogDbContext db) : ICatalogBrowsingRepository
{
    public Task<Store[]> StoresAsync(CancellationToken ct) => db.Stores.AsNoTracking()
        .Where(s => s.IsActive && s.PickupEnabled).OrderBy(s => s.Code).ToArrayAsync(ct);

    public Task<Category[]> CategoriesAsync(CancellationToken ct) => db.Categories.AsNoTracking()
        .Where(c => c.IsActive).OrderBy(c => c.Name).ToArrayAsync(ct);

    public async Task<PagedResult<Product>?> ProductsAsync(string code, int page, int pageSize,
        string? search, Guid? categoryId, decimal? minPrice, decimal? maxPrice, string sort, CancellationToken ct)
    {
        var store = await db.Stores.AsNoTracking().SingleOrDefaultAsync(s => s.Code == code, ct);
        if (store is null || !store.IsActive || !store.PickupEnabled) return null;
        var query = from p in db.Products.AsNoTracking()
                    join a in db.StoreAssortments.AsNoTracking() on p.Id equals a.ProductId
                    where a.StoreId == store.Id && a.IsListed && p.IsActive
                    select p;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[A-Za-z0-9][A-Za-z0-9._-]{0,49}$"))
            {
                var sku = Sku.Create(normalized);
                query = query.Where(p => p.Name.Contains(normalized) || p.Sku == sku);
            }
            else query = query.Where(p => p.Name.Contains(normalized));
        }
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (minPrice.HasValue) query = query.Where(p => p.BasePrice.Amount >= minPrice);
        if (maxPrice.HasValue) query = query.Where(p => p.BasePrice.Amount <= maxPrice);
        var total = await query.LongCountAsync(ct);
        var offset = (long)(page - 1) * pageSize;
        if (offset >= total) return new([], page, pageSize, total);
        var ordered = sort switch
        {
            "priceAsc" => query.OrderBy(p => p.BasePrice.Amount).ThenBy(p => p.Id),
            "priceDesc" => query.OrderByDescending(p => p.BasePrice.Amount).ThenBy(p => p.Id),
            _ => query.OrderBy(p => p.Name).ThenBy(p => p.Id)
        };
        return new(await ordered.Skip(checked((int)offset)).Take(pageSize).ToArrayAsync(ct), page, pageSize, total);
    }
}
