namespace Ecommerce_Api.Features.Inventory.Application.Dtos.Response;

public sealed record AvailabilityResponse(string StoreCode, string Sku, int RequestedQuantity,
    int AvailableQuantity, bool Sufficient, DateTimeOffset CheckedAtUtc);
