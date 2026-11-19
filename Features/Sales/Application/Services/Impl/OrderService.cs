using System.Text.Json;
using Ecommerce_Api.Features.Sales.Application.Dtos.Request;
using Ecommerce_Api.Features.Sales.Application.Services;
using Ecommerce_Api.Features.Sales.Domain.Entities;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Features.Sales.Application.Services.Impl;

public sealed class OrderService(IWorkflowStore store, IOrderQuoteProvider quotes, IPaymentEvidence payments,
    IPosReservationClient pos, IPickingIncidentWriter incidents, IConfiguration configuration) : IOrderService, IOrderPaymentSource, IConfirmedOrderSource
{
    public async Task<OrderWorkflow> CreateAsync(CreateOrderRequest request, string key, CancellationToken ct)
    {
        if (request.CustomerId == Guid.Empty || string.IsNullOrWhiteSpace(request.StoreCode) || request.Items is null || request.Items.Any(x => x is null)) throw new WorkflowException(400, "invalid_order");
        key = WorkflowRules.RequiredKey(key);
        var normalized = request with { StoreCode = request.StoreCode.Trim().ToUpperInvariant(), Items = request.Items.Select(x => x with { Sku = x.Sku?.Trim().ToUpperInvariant() ?? "" }).OrderBy(x => x.Sku).ToArray() };
        var fingerprint = WorkflowRules.Hash(JsonSerializer.Serialize(normalized));
        var id = WorkflowIds.Stable($"order:{request.CustomerId}:{key}");
        return await store.UpdateAsync<OrderWorkflow>("sales", id.ToString(), async current =>
        {
            if (current is not null)
            {
                if (current.Fingerprint != fingerprint) throw new WorkflowException(409, "idempotency_conflict");
                return current;
            }
            var quote = await quotes.QuoteAsync(normalized.StoreCode, normalized.Items, ct);
            return new(id, request.CustomerId, quote.StoreId, quote.StoreCode, quote.Items, quote.TotalAmount,
                quote.Currency, "AwaitingPayment", fingerprint, null, null, [], null, DateTimeOffset.UtcNow, null,
                [new("AwaitingPayment", "orchestrator", DateTimeOffset.UtcNow)]);
        }, ct);
    }

    public async Task<OrderWorkflow> GetAsync(Guid id, CancellationToken ct) =>
        await store.ReadAsync<OrderWorkflow>("sales", id.ToString(), ct) ?? throw new WorkflowException(404, "order_not_found");

    public async Task<OrderWorkflow> ConfirmAsync(Guid id, ConfirmOrderRequest request, CancellationToken ct) =>
        await store.UpdateAsync<OrderWorkflow>("sales", id.ToString(), async current =>
        {
            var order = current ?? throw new WorkflowException(404, "order_not_found");
            if (order.PaymentId is not null)
            {
                if (order.PaymentId != request.PaymentId || order.ReservationId != request.ReservationId) throw new WorkflowException(409, "confirmation_conflict");
                return order;
            }
            if (order.Status != "AwaitingPayment") throw new WorkflowException(409, "invalid_order_state");
            var payment = await payments.GetEvidenceAsync(request.PaymentId, ct);
            if (payment.Status != "Captured" || payment.OrderId != id || payment.ReservationId != request.ReservationId ||
                payment.Amount != order.TotalAmount || payment.Currency != order.Currency) throw new WorkflowException(409, "payment_not_verified");
            var reservation = await pos.GetAsync(request.ReservationId, ct);
            PosReservationRules.VerifyMatches(reservation, Source(order));
            if (reservation.Estado != "CONFIRMADA")
            {
                if (reservation.Estado != "ACTIVA" || reservation.FechaExpiracion <= DateTimeOffset.UtcNow) throw new WorkflowException(409, "reservation_expired");
                await pos.ConfirmAsync(request.ReservationId, request.PaymentId, ct);
                reservation = await pos.GetAsync(request.ReservationId, ct);
                if (reservation.Estado != "CONFIRMADA") throw new WorkflowException(409, "reservation_not_confirmed");
            }
            if (reservation.ReferenciaAutorizacionPago != $"SIMULATED-CAPTURE-{request.PaymentId}")
                throw new WorkflowException(409, "reservation_payment_reference_mismatch");
            return Transition(order with { PaymentId = request.PaymentId, ReservationId = request.ReservationId }, "Confirmed", "orchestrator");
        }, ct);

    public async Task<OrderWorkflow> StartPickingAsync(Guid id, string actor, CancellationToken ct) =>
        await store.UpdateAsync<OrderWorkflow>("sales", id.ToString(), order =>
        {
            if (order is null) throw new WorkflowException(404, "order_not_found");
            if (order.Status == "Preparing") return Task.FromResult(order);
            if (order.Status != "Confirmed") throw new WorkflowException(409, "invalid_order_state");
            return Task.FromResult(Transition(order, "Preparing", Actor(actor)));
        }, ct);

    public async Task<OrderWorkflow> PickAsync(Guid id, PickingRequest request, CancellationToken ct) =>
        await store.UpdateAsync<OrderWorkflow>("sales", id.ToString(), async current =>
        {
            var order = current ?? throw new WorkflowException(404, "order_not_found");
            var sku = request.Sku?.Trim().ToUpperInvariant();
            var line = order.Items.SingleOrDefault(x => x.Sku == sku) ?? throw new WorkflowException(400, "unknown_order_item");
            if (request.FoundQuantity < 0 || request.FoundQuantity > line.Quantity) throw new WorkflowException(400, "invalid_picking_quantity");
            if (order.Prepared.TryGetValue(line.Sku, out var prepared))
            {
                if (prepared != request.FoundQuantity) throw new WorkflowException(409, "picking_result_conflict");
                return order;
            }
            if (order.Status != "Preparing") throw new WorkflowException(409, "invalid_order_state");
            var actor = Actor(request.OperatorId);
            order.Prepared.Add(line.Sku, request.FoundQuantity);
            if (request.FoundQuantity < line.Quantity)
            {
                await incidents.RecordShortageAsync(id, order.StoreCode, line.Sku, line.Quantity - request.FoundQuantity, actor, ct);
                return Transition(order, "Incident", actor);
            }
            return order;
        }, ct);

    public async Task<object> ReadyAsync(Guid id, string actor, CancellationToken ct)
    {
        var code = PickupCode(id);
        var order = await store.UpdateAsync<OrderWorkflow>("sales", id.ToString(), current =>
        {
            var found = current ?? throw new WorkflowException(404, "order_not_found");
            if (found.Status == "ReadyForPickup") return Task.FromResult(found);
            if (found.Status != "Preparing" || found.Items.Any(x => !found.Prepared.TryGetValue(x.Sku, out var qty) || qty != x.Quantity)) throw new WorkflowException(409, "picking_incomplete");
            return Task.FromResult(Transition(found with { PickupCodeHash = WorkflowRules.Hash(code) }, "ReadyForPickup", Actor(actor)));
        }, ct);
        return new { order.OrderId, order.Status, pickupCode = code };
    }

    public async Task<OrderWorkflow> CollectAsync(Guid id, PickupRequest request, CancellationToken ct) =>
        await store.UpdateAsync<OrderWorkflow>("sales", id.ToString(), current =>
        {
            var order = current ?? throw new WorkflowException(404, "order_not_found");
            if (order.StoreCode != request.StoreCode?.Trim().ToUpperInvariant() || order.PickupCodeHash != WorkflowRules.Hash(request.Code ?? "")) throw new WorkflowException(403, "invalid_pickup_evidence");
            if (order.Status == "Collected") return Task.FromResult(order);
            if (order.Status != "ReadyForPickup") throw new WorkflowException(409, "order_not_ready");
            return Task.FromResult(Transition(order with { CollectedAtUtc = DateTimeOffset.UtcNow }, "Collected", Actor(request.OperatorId)));
        }, ct);

    public async Task<OrderPaymentSource> GetPaymentSourceAsync(Guid orderId, CancellationToken ct) => Source(await GetAsync(orderId, ct));
    public async Task<ConfirmedOrder> GetConfirmedAsync(Guid orderId, CancellationToken ct)
    {
        var order = await GetAsync(orderId, ct);
        if (order.PaymentId is not Guid paymentId || order.Status is "AwaitingPayment" or "Cancelled") throw new WorkflowException(409, "order_not_confirmed");
        return new(order.OrderId, paymentId, order.StoreCode, order.TotalAmount, order.Currency, order.Status);
    }
    private string PickupCode(Guid id)
    {
        var secret = configuration["Operator:PickupSecret"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32) throw new WorkflowException(503, "pickup_secret_not_configured");
        return Convert.ToHexString(System.Security.Cryptography.HMACSHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secret), id.ToByteArray()))[..12];
    }
    private static OrderPaymentSource Source(OrderWorkflow order) => new(order.OrderId, order.CustomerId, order.StoreCode, order.Items, order.TotalAmount, order.Currency, order.Status);
    private static string Actor(string actor) => string.IsNullOrWhiteSpace(actor) || actor.Length > 100 ? throw new WorkflowException(400, "operator_required") : actor;
    private static OrderWorkflow Transition(OrderWorkflow order, string status, string actor)
    {
        order.History.Add(new(status, actor, DateTimeOffset.UtcNow));
        return order with { Status = status };
    }
}
