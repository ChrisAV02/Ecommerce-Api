using Ecommerce_Api.Features.Catalog.Domain.Repositories;
using Ecommerce_Api.Features.Catalog.Domain.Entities;
using Ecommerce_Api.Features.Catalog.Domain.ValueObjects;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Repositories.Impl;

// Fictitious development-only data, created once per process. Never persisted to PostgreSQL.
public sealed class InMemoryProductRepository : IProductRepository
{
    private static readonly Product[] Products =
    [
        Product.Create(Sku.Create("DEMO-001"), "Arroz Extra 1 kg (DEMO)",
            "Producto ficticio para demostración; no confirma stock.",
            Guid.Parse("e0e209ac-3026-490f-8dad-f636491b0668"), Money.Create(4.90m, "PEN"),
            new DateTimeOffset(2026, 11, 17, 0, 0, 0, TimeSpan.Zero))
    ];

    public Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var offset = (long)(query.Page - 1) * query.PageSize;
        IReadOnlyList<Product> items = offset >= Products.Length
            ? [] : Products.Skip((int)offset).Take(query.PageSize).ToArray();
        return Task.FromResult(new PagedResult<Product>(items, query.Page, query.PageSize, Products.Length));
    }

}
