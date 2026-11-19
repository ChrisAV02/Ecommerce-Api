namespace Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

public sealed record CatalogSynchronizationResponseDto(
    Guid SynchronizationId, string Status, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt,
    int SourceProducts, int SourceStores, int SourceAssortments, int ProductsCreated, int ProductsUpdated,
    int CategoriesCreated, int StoresCreated, int AssortmentsCreated, IReadOnlyList<string> MissingPriceSkus,
    string PriceSource, string? ErrorCode);
