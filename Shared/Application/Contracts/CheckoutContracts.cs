using Ecommerce_Api.Shared.Application;

namespace Ecommerce_Api.Shared.Application.Contracts;

public sealed record OrderPaymentSource(Guid OrderId, Guid CustomerId, string StoreCode, IReadOnlyList<QuotedLine> Items,
    decimal TotalAmount, string Currency, string Status);
public interface IOrderPaymentSource
{
    Task<OrderPaymentSource> GetPaymentSourceAsync(Guid orderId, CancellationToken ct);
}
public sealed record CapturedPayment(Guid PaymentId, Guid OrderId, Guid ReservationId, decimal Amount, string Currency, string Status);
public interface IPaymentEvidence
{
    Task<CapturedPayment> GetEvidenceAsync(Guid paymentId, CancellationToken ct);
}
public sealed record VerifiedRefund(Guid RefundId, Guid PaymentId, decimal Amount);
public interface IRefundEvidence
{
    Task<VerifiedRefund> GetRefundAsync(Guid paymentId, Guid refundId, CancellationToken ct);
}
public sealed record ConfirmedOrder(Guid OrderId, Guid PaymentId, string StoreCode, decimal Amount, string Currency, string Status);
public interface IConfirmedOrderSource
{
    Task<ConfirmedOrder> GetConfirmedAsync(Guid orderId, CancellationToken ct);
}
public interface IOrderCompensationSource
{
    Task CancelForRefundAsync(Guid orderId, CancellationToken ct);
}
public interface IPickingIncidentWriter
{
    Task<Guid> RecordShortageAsync(Guid orderId, string storeCode, string sku, int quantity, string actor, CancellationToken ct);
}

public static class WorkflowIds
{
    public static Guid Stable(string value) => new(Convert.FromHexString(WorkflowRules.Hash(value))[..16]);
}
