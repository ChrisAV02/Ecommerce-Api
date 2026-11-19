using Ecommerce_Api.Features.Sales.Domain.Entities;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Features.Sales.Infrastructure.Repositories.Impl;

public sealed class OrderReadSource(IWorkflowStore store) : IOrderPaymentSource, IConfirmedOrderSource, IOrderCompensationSource
{
    public async Task<OrderPaymentSource> GetPaymentSourceAsync(Guid id, CancellationToken ct)
    {
        var order = await ReadAsync(id, ct);
        return new(order.OrderId, order.CustomerId, order.StoreCode, order.Items, order.TotalAmount, order.Currency, order.Status);
    }
    public async Task<ConfirmedOrder> GetConfirmedAsync(Guid id, CancellationToken ct)
    {
        var order = await ReadAsync(id, ct);
        if (order.PaymentId is not Guid paymentId || order.Status is "AwaitingPayment" or "Cancelled") throw new WorkflowException(409, "order_not_confirmed");
        return new(order.OrderId, paymentId, order.StoreCode, order.TotalAmount, order.Currency, order.Status);
    }
    private async Task<OrderWorkflow> ReadAsync(Guid id, CancellationToken ct) =>
        await store.ReadAsync<OrderWorkflow>("sales", id.ToString(), ct) ?? throw new WorkflowException(404, "order_not_found");

    public async Task CancelForRefundAsync(Guid orderId, CancellationToken ct)
    {
        // Sales state is committed before refund. Confirmation uses the same module-owned lock,
        // so a refunded payment cannot concurrently finalize an AwaitingPayment order.
        await store.UpdateAsync<OrderWorkflow>("sales", orderId.ToString(), current =>
        {
            var order = current ?? throw new WorkflowException(404, "order_not_found");
            if (order.Status == "Cancelled") return Task.FromResult(order);
            if (order.Status is not ("AwaitingPayment" or "Incident")) throw new WorkflowException(409, "refund_requires_unconfirmed_order_or_incident");
            order.History.Add(new("Cancelled", "payment-compensation", DateTimeOffset.UtcNow));
            return Task.FromResult(order with { Status = "Cancelled" });
        }, ct);
    }
}
