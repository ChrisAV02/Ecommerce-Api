namespace Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

public sealed record StoreResponse(Guid StoreId, string StoreCode, string Name, string Address,
    decimal? Latitude, decimal? Longitude, double? ApproximateDistanceKm);

public sealed record CategoryResponse(Guid CategoryId, string Name);

public sealed record StoreProductResponse(Guid ProductId, string Sku, string Name, string? Description,
    Guid CategoryId, PriceResponse BasePrice, bool IsActive);

public sealed record StoreCatalogResponse(string StoreCode, IReadOnlyList<StoreProductResponse> Items, int Page, int PageSize, long TotalCount);
