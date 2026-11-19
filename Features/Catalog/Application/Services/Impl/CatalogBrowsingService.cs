using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;
using Ecommerce_Api.Features.Catalog.Application.Services;
using Ecommerce_Api.Features.Catalog.Domain.Repositories;

namespace Ecommerce_Api.Features.Catalog.Application.Services.Impl;

public sealed class CatalogBrowsingService(ICatalogBrowsingRepository repository) : ICatalogBrowsingService
{
    public async Task<IReadOnlyList<StoreResponse>> StoresAsync(decimal? latitude, decimal? longitude, CancellationToken ct)
    {
        var stores = await repository.StoresAsync(ct);
        return stores.Select(s => new StoreResponse(s.Id, s.Code, s.Name, s.Address, s.Location?.Latitude, s.Location?.Longitude,
            latitude.HasValue && longitude.HasValue && s.Location is not null
                ? Distance((double)latitude.Value, (double)longitude.Value, (double)s.Location.Latitude, (double)s.Location.Longitude) : null))
            .OrderBy(s => s.ApproximateDistanceKm ?? double.MaxValue).ThenBy(s => s.StoreCode).ToArray();
    }

    public async Task<IReadOnlyList<CategoryResponse>> CategoriesAsync(CancellationToken ct) =>
        (await repository.CategoriesAsync(ct)).Select(c => new CategoryResponse(c.Id, c.Name)).ToArray();

    public async Task<StoreCatalogResponse?> ProductsAsync(string storeCode, int page, int pageSize, string? search,
        Guid? categoryId, decimal? minPrice, decimal? maxPrice, string sort, CancellationToken ct)
    {
        var code = storeCode.Trim().ToUpperInvariant();
        var result = await repository.ProductsAsync(code, page, pageSize, search, categoryId, minPrice, maxPrice, sort, ct);
        return result is null ? null : new(code, result.Items.Select(p => new StoreProductResponse(p.Id, p.Sku.Value,
            p.Name, p.Description, p.CategoryId, new(p.BasePrice.Amount, p.BasePrice.Currency), p.IsActive)).ToArray(), page, pageSize, result.TotalCount);
    }

    // Haversine: straight-line approximation, never a driving distance.
    private static double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        static double Radians(double x) => x * Math.PI / 180;
        var a = Math.Pow(Math.Sin(Radians(lat2 - lat1) / 2), 2)
            + Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * Math.Pow(Math.Sin(Radians(lon2 - lon1) / 2), 2);
        return Math.Round(6371 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1))), 3);
    }
}
