using Ecommerce_Api.Features.Payments.Application.Services;
using Ecommerce_Api.Features.Sales.Application.Dtos.Request;
using Ecommerce_Api.Features.Sales.Application.Dtos.Response;
using Ecommerce_Api.Features.Sales.Application.Services;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Sales.Controllers;

[ApiController, Route("api/v1/sales/orders"), Tags("Sales"), WorkflowKey, ServiceFilter(typeof(WorkflowExceptionFilter))]
public sealed class SalesController(IOrderService orders, IPaymentService payments) : ControllerBase
{
    [HttpPost]
    public async Task<OrderResponse> Create(CreateOrderRequest request, CancellationToken ct) => OrderResponse.From(await orders.CreateAsync(request, Request.Headers["Idempotency-Key"].ToString(), ct));
    [HttpGet("{id:guid}")]
    public async Task<OrderResponse> Get(Guid id, CancellationToken ct) => OrderResponse.From(await orders.GetAsync(id, ct));
    [HttpPost("{id:guid}/confirm")]
    public async Task<OrderResponse> Confirm(Guid id, ConfirmOrderRequest request, CancellationToken ct) => OrderResponse.From(await orders.ConfirmAsync(id, request, ct));
    [HttpPost("{id:guid}/cancel")]
    public Task<OrderPaymentSource> Cancel(Guid id, CancelOrderRequest request, CancellationToken ct) => payments.CancelBeforePaymentAsync(id, request.Reason, ct);
    [HttpPost("{id:guid}/picking/start"), WorkflowKey(true)]
    public async Task<OrderResponse> Start(Guid id, OperatorRequest request, CancellationToken ct) => OrderResponse.From(await orders.StartPickingAsync(id, request.OperatorId, ct));
    [HttpPost("{id:guid}/picking/items"), WorkflowKey(true)]
    public async Task<OrderResponse> Pick(Guid id, PickingRequest request, CancellationToken ct) => OrderResponse.From(await orders.PickAsync(id, request, ct));
    [HttpPost("{id:guid}/ready"), WorkflowKey(true)]
    public Task<object> Ready(Guid id, OperatorRequest request, CancellationToken ct) => orders.ReadyAsync(id, request.OperatorId, ct);
    [HttpPost("{id:guid}/collect"), WorkflowKey(true)]
    public async Task<OrderResponse> Collect(Guid id, PickupRequest request, CancellationToken ct) => OrderResponse.From(await orders.CollectAsync(id, request, ct));
}
