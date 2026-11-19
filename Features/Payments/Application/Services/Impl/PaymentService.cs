using System.Text.Json;
using Ecommerce_Api.Features.Payments.Application.Dtos.Request;
using Ecommerce_Api.Features.Payments.Application.Dtos.Response;
using Ecommerce_Api.Features.Payments.Application.Services;
using Ecommerce_Api.Features.Payments.Domain.Entities;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Features.Payments.Application.Services.Impl;

public sealed class PaymentService(IWorkflowStore store, IOrderPaymentSource orders, IOrderCompensationSource compensation,
    IPosReservationClient pos, IHostEnvironment environment) : IPaymentService
{
    public Task<PaymentResponse> GetAsync(Guid id, CancellationToken ct) => GetResponseAsync(id, ct);
    private async Task<PaymentResponse> GetResponseAsync(Guid id, CancellationToken ct) =>
        Map((await store.ReadAsync<PaymentWorkflow>("payments", id.ToString(), ct) ?? throw new WorkflowException(404, "payment_not_found")).Current);

    public async Task<PaymentResponse> PayAsync(PaymentRequest request, string key, CancellationToken ct)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing")) throw new WorkflowException(503, "simulated_payments_disabled");
        key = WorkflowRules.RequiredKey(key);
        if (request.OrderId == Guid.Empty || request.ReservationId == Guid.Empty || request.Scenario is not ("Approved" or "Rejected" or "Pending" or "Timeout" or "DuplicatedCallback" or "ProviderUnavailable")) throw new WorkflowException(400, "invalid_payment_request");
        var id = WorkflowIds.Stable($"payment:{request.OrderId}");
        var fingerprint = WorkflowRules.Hash(JsonSerializer.Serialize(request));
        PaymentState? response = null;
        await store.UpdateAsync<PaymentWorkflow>("payments", id.ToString(), async current =>
        {
            if (current?.Attempts.TryGetValue(key, out var replay) == true)
            {
                if (replay.Fingerprint != fingerprint) throw new WorkflowException(409, "idempotency_conflict");
                response = replay.Response;
                return current;
            }
            if (current?.Current.Status is "Captured" or "Refunded") throw new WorkflowException(409, "payment_already_processed");
            if (current?.Current.Status is "Pending" or "Unknown") throw new WorkflowException(409, "payment_reconciliation_required");
            if (current is not null && current.Current.ReservationId != request.ReservationId) throw new WorkflowException(409, "payment_reservation_mismatch");
            var order = await orders.GetPaymentSourceAsync(request.OrderId, ct);
            if (order.Status != "AwaitingPayment" || order.TotalAmount <= 0 || decimal.Round(order.TotalAmount, 2) != order.TotalAmount) throw new WorkflowException(409, "order_not_payable");
            var reservation = await pos.GetAsync(request.ReservationId, ct);
            PosReservationRules.VerifyMatches(reservation, order);
            if (reservation.Estado != "ACTIVA" || reservation.FechaExpiracion <= DateTimeOffset.UtcNow) throw new WorkflowException(409, "reservation_expired");
            if (request.Scenario == "ProviderUnavailable") throw new WorkflowException(503, "simulated_provider_unavailable");
            var status = request.Scenario switch { "Approved" or "DuplicatedCallback" => "Captured", "Rejected" => "Rejected", "Pending" => "Pending", _ => "Unknown" };
            response = new(id, request.OrderId, request.ReservationId, status, order.TotalAmount, order.Currency);
            var next = current ?? new PaymentWorkflow(response, [], [], DateTimeOffset.UtcNow);
            next.Attempts.Add(key, new(fingerprint, response));
            return next with { Current = response };
        }, ct);
        return Map(response!);
    }

    public async Task<PaymentResponse> ReconcileAsync(Guid id, CancellationToken ct)
    {
        var result = await store.UpdateAsync<PaymentWorkflow>("payments", id.ToString(), current =>
        {
            var payment = current ?? throw new WorkflowException(404, "payment_not_found");
            // Simulated provider's deterministic remote result: Pending/Timeout eventually captured once.
            // A late capture still requires Sales reservation checks and Saga compensation.
            if (payment.Current.Status is "Pending" or "Unknown") payment = payment with { Current = payment.Current with { Status = "Captured" } };
            return Task.FromResult(payment);
        }, ct);
        return Map(result.Current);
    }

    public async Task<OrderPaymentSource> CancelBeforePaymentAsync(Guid orderId, string reason, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500) throw new WorkflowException(400, "cancellation_reason_required");
        var id = WorkflowIds.Stable($"payment:{orderId}");
        await store.UpdateAsync<PaymentWorkflow>("payments", id.ToString(), async current =>
        {
            if (current?.Current.Status is "Captured" or "Refunded" or "Pending" or "Unknown") throw new WorkflowException(409, "payment_must_be_reconciled_before_cancellation");
            var order = await orders.GetPaymentSourceAsync(orderId, ct);
            if (order.Status is not ("AwaitingPayment" or "Cancelled")) throw new WorkflowException(409, "order_not_cancellable");
            await compensation.CancelForRefundAsync(orderId, ct);
            return (current ?? new PaymentWorkflow(new(id, orderId, Guid.Empty, "Cancelled", order.TotalAmount, order.Currency), [], [], DateTimeOffset.UtcNow)) with
            { Current = new(id, orderId, current?.Current.ReservationId ?? Guid.Empty, "Cancelled", order.TotalAmount, order.Currency) };
        }, ct);
        return await orders.GetPaymentSourceAsync(orderId, ct);
    }

    public async Task<RefundResponse> RefundAsync(Guid id, RefundRequest request, string key, CancellationToken ct)
    {
        key = WorkflowRules.RequiredKey(key);
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500) throw new WorkflowException(400, "refund_reason_required");
        var fingerprint = WorkflowRules.Hash(JsonSerializer.Serialize(request));
        var snapshot = await store.ReadAsync<PaymentWorkflow>("payments", id.ToString(), ct) ?? throw new WorkflowException(404, "payment_not_found");
        if (!snapshot.Refunds.ContainsKey(key))
        {
            var balance = snapshot.Current.Amount - snapshot.Refunds.Values.Sum(x => x.Response.Amount);
            var requested = request.Amount ?? balance;
            if (snapshot.Current.Status != "Captured" || requested <= 0 || requested > balance || decimal.Round(requested, 2) != requested)
                throw new WorkflowException(409, "refund_exceeds_capture");
            await compensation.CancelForRefundAsync(snapshot.Current.OrderId, ct);
        }
        RefundState? response = null;
        await store.UpdateAsync<PaymentWorkflow>("payments", id.ToString(), current =>
        {
            var payment = current ?? throw new WorkflowException(404, "payment_not_found");
            if (payment.Refunds.TryGetValue(key, out var prior))
            {
                if (prior.Fingerprint != fingerprint) throw new WorkflowException(409, "idempotency_conflict");
                response = prior.Response;
                return Task.FromResult(payment);
            }
            var remaining = payment.Current.Amount - payment.Refunds.Values.Sum(x => x.Response.Amount);
            var amount = request.Amount ?? remaining;
            if (payment.Current.Status != "Captured" || amount <= 0 || amount > remaining || decimal.Round(amount, 2) != amount) throw new WorkflowException(409, "refund_exceeds_capture");
            response = new(WorkflowIds.Stable($"refund:{id}:{key}"), id, amount);
            payment.Refunds.Add(key, new(fingerprint, response));
            if (amount == remaining) payment = payment with { Current = payment.Current with { Status = "Refunded" } };
            return Task.FromResult(payment);
        }, ct);
        return Map(response!);
    }

    private static PaymentResponse Map(PaymentState state) => new(state.PaymentId, state.OrderId, state.ReservationId,
        state.Status, state.Amount, state.Currency, state.Simulated);
    private static RefundResponse Map(RefundState state) => new(state.RefundId, state.PaymentId, state.Amount,
        state.Status, state.Simulated);
}
