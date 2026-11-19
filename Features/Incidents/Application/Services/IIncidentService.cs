using Ecommerce_Api.Features.Incidents.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Incidents.Application.Services;

public interface IIncidentService
{
    Task<Guid> RecordShortageAsync(Guid orderId, string storeCode, string sku, int quantity, string actor, CancellationToken ct);
    Task<PickingIncidentResponse> GetAsync(Guid id, CancellationToken ct);
}
