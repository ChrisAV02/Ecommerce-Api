namespace Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

public sealed record ProductListResponseDto(
    IReadOnlyList<ProductResponseDto> Items, int Page, int PageSize, long TotalCount);
