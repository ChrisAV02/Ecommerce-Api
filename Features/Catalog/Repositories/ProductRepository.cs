using Ecommerce_Api.Features.Catalog.DTOs;
using Ecommerce_Api.Features.Catalog.Models;
using Ecommerce_Api.Features.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Features.Catalog.Repositories;

public sealed class ProductRepository(CatalogDbContext dbContext) : IProductRepository
{
    public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var products = dbContext.Products.AsNoTracking();
        var totalCount = await products.LongCountAsync(cancellationToken);
        var offset = (long)(query.Page - 1) * query.PageSize;
        // An out-of-range page is empty, including offsets beyond EF's Int32 Skip limit.
        if (offset >= totalCount) return new([], query.Page, query.PageSize, totalCount);
        if (offset > int.MaxValue)
        {
            // PostgreSQL OFFSET is Int64; EF's Skip accepts only Int32.
            var largePage = await dbContext.Products.FromSqlInterpolated($"""
                SELECT * FROM catalog.product
                ORDER BY name, product_id OFFSET {offset} LIMIT {query.PageSize}
                """)
                .AsNoTracking().ToArrayAsync(cancellationToken);
            return new(largePage, query.Page, query.PageSize, totalCount);
        }
        var items = await products.OrderBy(product => product.Name).ThenBy(product => product.Id)
            .Skip((int)offset).Take(query.PageSize).ToArrayAsync(cancellationToken);
        return new(items, query.Page, query.PageSize, totalCount);
    }

}


