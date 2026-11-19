using Ecommerce_Api.Features.Sales.Application.Dtos.Request;
using Ecommerce_Api.Features.Sales.Domain.Entities;

namespace Ecommerce_Api.Features.Sales.Application.Services;

public interface IOrderService
{
    Task<OrderWorkflow> CreateAsync(CreateOrderRequest request, string key, CancellationToken ct);
    Task<OrderWorkflow> GetAsync(Guid id, CancellationToken ct);
    Task<OrderWorkflow> ConfirmAsync(Guid id, ConfirmOrderRequest request, CancellationToken ct);
    Task<OrderWorkflow> StartPickingAsync(Guid id, string actor, CancellationToken ct);
    Task<OrderWorkflow> PickAsync(Guid id, PickingRequest request, CancellationToken ct);
    Task<object> ReadyAsync(Guid id, string actor, CancellationToken ct);
    Task<OrderWorkflow> CollectAsync(Guid id, PickupRequest request, CancellationToken ct);
}
