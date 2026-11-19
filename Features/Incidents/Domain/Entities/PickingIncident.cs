namespace Ecommerce_Api.Features.Incidents.Domain.Entities;

public sealed record PickingIncident(Guid IncidentId, Guid OrderId, string StoreCode, string Sku,
    int MissingQuantity, string OperatorId, string Status, DateTimeOffset CreatedAtUtc);
