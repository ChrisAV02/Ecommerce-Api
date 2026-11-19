namespace Ecommerce_Api.Features.Incidents.Application.Dtos.Response;

public sealed record PickingIncidentResponse(Guid IncidentId, Guid OrderId, string StoreCode, string Sku,
    int MissingQuantity, string OperatorId, string Status, DateTimeOffset CreatedAtUtc);
