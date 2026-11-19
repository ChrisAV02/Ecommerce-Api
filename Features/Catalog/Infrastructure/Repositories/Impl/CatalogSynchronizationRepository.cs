using System.Text.Json;
using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;
using Ecommerce_Api.Infrastructure.Persistence;
using Ecommerce_Api.Features.Catalog.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Repositories.Impl;

public sealed class CatalogSynchronizationRepository(CatalogDbContext db) : ICatalogSynchronizationStore
{
    public async Task<CatalogSynchronizationRun> StartAsync(CancellationToken ct)
    {
        var run = CatalogSynchronizationRun.Start(DateTimeOffset.UtcNow);
        db.SynchronizationRuns.Add(run);
        await db.SaveChangesAsync(ct);
        return run;
    }

    public Task<CatalogSynchronizationRun?> FindAsync(Guid id, CancellationToken ct) =>
        db.SynchronizationRuns.AsNoTracking().SingleOrDefaultAsync(run => run.Id == id, ct);

    public async Task FailAsync(Guid id, string code, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        var run = await db.SynchronizationRuns.SingleAsync(run => run.Id == id, ct);
        run.Fail(code);
        await db.SaveChangesAsync(ct);
    }

    public async Task ApplyAsync(Guid runId, CatalogSourceSnapshot source, IReadOnlyDictionary<string, decimal> prices, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // All synchronization writers share a single Catalog-owned transaction lock.
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(694104)", ct);
        var run = await db.SynchronizationRuns.SingleAsync(run => run.Id == runId, ct);
        var products = (await db.Products.ToListAsync(ct)).ToDictionary(product => product.Sku.Value, StringComparer.Ordinal);
        var stores = (await db.Stores.ToListAsync(ct)).ToDictionary(store => store.Code, StringComparer.Ordinal);
        var categories = (await db.Categories.ToListAsync(ct)).GroupBy(category => category.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(category => category.Id).First(), StringComparer.Ordinal);
        var assortments = (await db.StoreAssortments.ToListAsync(ct)).ToDictionary(item => (item.StoreId, item.ProductId));
        var now = DateTimeOffset.UtcNow;
        var productsCreated = 0; var productsUpdated = 0; var categoriesCreated = 0; var storesCreated = 0; var assortmentsCreated = 0;
        var missingPrices = new List<string>();
        foreach (var item in source.Products)
        {
            var sku = Sku.Create(item.Sku);
            if (!products.TryGetValue(sku.Value, out var product))
            {
                if (!prices.TryGetValue(sku.Value, out var amount)) { missingPrices.Add(sku.Value); continue; }
                var price = Money.Create(amount, "PEN");
                var categoryName = item.Categoria.Trim();
                if (!categories.TryGetValue(categoryName, out var category))
                {
                    category = Category.Create(categoryName, null, now);
                    categories.Add(categoryName, category); db.Categories.Add(category); categoriesCreated++;
                }
                product = Product.Create(sku, item.Nombre, item.Descripcion, category.Id, price, now);
                products.Add(sku.Value, product); db.Products.Add(product); productsCreated++;
            }
            // Preserve local names, descriptions, category assignments and commercial prices.
            // POS only controls activity of an existing SKU.
            if (product.IsActive != item.Activo)
            {
                if (item.Activo) product.Activate(now); else product.Deactivate(now);
                productsUpdated++;
            }
        }
        foreach (var item in source.Stores)
        {
            var code = item.Codigo.Trim().ToUpperInvariant();
            if (!stores.TryGetValue(code, out var store))
            {
                var location = item.Latitud.HasValue && item.Longitud.HasValue
                    ? GeoCoordinate.Create(item.Latitud.Value, item.Longitud.Value) : null;
                store = Store.Create(code, item.Nombre, item.Direccion, location, pickupEnabled: true, deliveryEnabled: false, now);
                stores.Add(code, store); db.Stores.Add(store); storesCreated++;
            }
            if (store.IsActive != item.Activa) { if (item.Activa) store.Activate(now); else store.Deactivate(now); }
            if (!source.Assortments.TryGetValue(item.Codigo, out var sourceItems))
            {
                if (!store.IsActive)
                    foreach (var existing in assortments.Values.Where(assortment => assortment.StoreId == store.Id && assortment.IsListed))
                        existing.Unlist(now);
                continue;
            }
            var listed = new HashSet<Guid>();
            foreach (var sourceItem in sourceItems)
            {
                if (!products.TryGetValue(Sku.Create(sourceItem.Sku).Value, out var product)) continue;
                listed.Add(product.Id);
                if (!assortments.TryGetValue((store.Id, product.Id), out var assortment))
                {
                    assortment = StoreAssortment.Create(store.Id, product.Id, now);
                    assortments.Add((store.Id, product.Id), assortment); db.StoreAssortments.Add(assortment); assortmentsCreated++;
                }
                var shouldList = store.IsActive && product.IsActive;
                if (assortment.IsListed != shouldList) { if (shouldList) assortment.List(now); else assortment.Unlist(now); }
            }
            foreach (var existing in assortments.Values.Where(assortment => assortment.StoreId == store.Id && !listed.Contains(assortment.ProductId) && assortment.IsListed))
                existing.Unlist(now);
        }
        run.Complete(source.Products.Count, source.Stores.Count, source.Assortments.Values.Sum(items => items.Count), productsCreated,
            productsUpdated, categoriesCreated, storesCreated, assortmentsCreated, JsonSerializer.Serialize(missingPrices.Order().ToArray()));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
