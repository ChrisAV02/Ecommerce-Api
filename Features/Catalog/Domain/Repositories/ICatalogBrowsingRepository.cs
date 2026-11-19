using Ecommerce_Api.Features.Catalog.Domain.Entities;

namespace Ecommerce_Api.Features.Catalog.Domain.Repositories;

public interface ICatalogBrowsingRepository
{
    Task<Store[]> StoresAsync(CancellationToken ct);
    Task<Category[]> CategoriesAsync(CancellationToken ct);
    Task<PagedResult<Product>?> ProductsAsync(string code, int page, int pageSize,
        string? search, Guid? categoryId, decimal? minPrice, decimal? maxPrice, string sort, CancellationToken ct);
}
