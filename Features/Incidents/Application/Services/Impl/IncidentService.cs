using Ecommerce_Api.Features.Incidents.Application.Dtos.Response;
using Ecommerce_Api.Features.Incidents.Application.Services;
using Ecommerce_Api.Features.Incidents.Domain.Entities;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Features.Incidents.Application.Services.Impl;

public sealed class IncidentService(IWorkflowStore store) : IIncidentService, IPickingIncidentWriter
{
    public async Task<Guid> RecordShortageAsync(Guid orderId, string storeCode, string sku, int quantity, string actor, CancellationToken ct)
    {
        if (quantity <= 0) throw new WorkflowException(400, "invalid_shortage_quantity");
        var id = WorkflowIds.Stable($"shortage:{orderId}:{sku}");
        await store.UpdateAsync<PickingIncident>("incidents", id.ToString(), current =>
        {
            if (current is not null && (current.MissingQuantity != quantity || current.StoreCode != storeCode)) throw new WorkflowException(409, "shortage_conflict");
            return Task.FromResult(current ?? new(id, orderId, storeCode, sku, quantity, actor, "Open", DateTimeOffset.UtcNow));
        }, ct);
        return id;
    }

    public async Task<PickingIncidentResponse> GetAsync(Guid id, CancellationToken ct) =>
        Map(await store.ReadAsync<PickingIncident>("incidents", id.ToString(), ct) ?? throw new WorkflowException(404, "incident_not_found"));

    private static PickingIncidentResponse Map(PickingIncident incident) => new(incident.IncidentId, incident.OrderId,
        incident.StoreCode, incident.Sku, incident.MissingQuantity, incident.OperatorId, incident.Status, incident.CreatedAtUtc);
}
