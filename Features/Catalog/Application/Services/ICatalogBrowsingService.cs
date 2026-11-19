using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Catalog.Application.Services;

public interface ICatalogBrowsingService
{
    Task<IReadOnlyList<StoreResponse>> StoresAsync(decimal? latitude, decimal? longitude, CancellationToken ct);
    Task<IReadOnlyList<CategoryResponse>> CategoriesAsync(CancellationToken ct);
    Task<StoreCatalogResponse?> ProductsAsync(string storeCode, int page, int pageSize, string? search,
        Guid? categoryId, decimal? minPrice, decimal? maxPrice, string sort, CancellationToken ct);
}
