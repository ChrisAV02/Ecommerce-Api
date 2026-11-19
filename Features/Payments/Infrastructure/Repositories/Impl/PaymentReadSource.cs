using Ecommerce_Api.Features.Payments.Domain.Entities;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Features.Payments.Infrastructure.Repositories.Impl;

public sealed class PaymentReadSource(IWorkflowStore store) : IPaymentEvidence, IRefundEvidence
{
    public async Task<CapturedPayment> GetEvidenceAsync(Guid paymentId, CancellationToken ct)
    {
        var payment = (await store.ReadAsync<PaymentWorkflow>("payments", paymentId.ToString(), ct) ?? throw new WorkflowException(404, "payment_not_found")).Current;
        var workflow = await store.ReadAsync<PaymentWorkflow>("payments", paymentId.ToString(), ct) ?? throw new WorkflowException(404, "payment_not_found");
        var status = workflow.Refunds.Count > 0 && payment.Status == "Captured" ? "PartiallyRefunded" : payment.Status;
        return new(payment.PaymentId, payment.OrderId, payment.ReservationId, payment.Amount, payment.Currency, status);
    }
    public async Task<VerifiedRefund> GetRefundAsync(Guid paymentId, Guid refundId, CancellationToken ct)
    {
        var workflow = await store.ReadAsync<PaymentWorkflow>("payments", paymentId.ToString(), ct) ?? throw new WorkflowException(404, "payment_not_found");
        var refund = workflow.Refunds.Values.SingleOrDefault(x => x.Response.RefundId == refundId)?.Response ?? throw new WorkflowException(404, "refund_not_found");
        return new(refund.RefundId, refund.PaymentId, refund.Amount);
    }
}
