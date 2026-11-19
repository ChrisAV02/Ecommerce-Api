namespace Ecommerce_Api.Features.Catalog.DTOs.Responses;

public sealed record ProductListResponseDto(
    IReadOnlyList<ProductResponseDto> Items, int Page, int PageSize, long TotalCount);
