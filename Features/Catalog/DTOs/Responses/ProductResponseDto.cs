namespace Ecommerce_Api.Features.Catalog.DTOs.Responses;

public sealed record ProductResponseDto(
    Guid ProductId, string Sku, string Name, string? Description,
    PriceResponse BasePrice, bool IsActive);
