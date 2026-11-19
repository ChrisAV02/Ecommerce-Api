using Ecommerce_Api.Features.Inventory.Application.Dtos.Request;

namespace Ecommerce_Api.Features.Inventory.Application.Dtos.Response;

public sealed record ReservationResponse(Guid ReservationId, string StoreCode, string OrderId, string Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc, IReadOnlyList<ReservationItem> Items);
