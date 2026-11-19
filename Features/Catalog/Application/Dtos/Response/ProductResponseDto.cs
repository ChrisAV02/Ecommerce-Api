namespace Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

public sealed record ProductResponseDto(
    Guid ProductId, string Sku, string Name, string? Description,
    PriceResponse BasePrice, bool IsActive);
