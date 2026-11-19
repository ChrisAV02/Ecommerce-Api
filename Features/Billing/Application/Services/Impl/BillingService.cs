using Ecommerce_Api.Features.Billing.Application.Dtos.Request;
using Ecommerce_Api.Features.Billing.Application.Dtos.Response;
using Ecommerce_Api.Features.Billing.Application.Services;
using Ecommerce_Api.Features.Billing.Domain.Entities;
using Ecommerce_Api.Shared.Application;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Features.Billing.Application.Services.Impl;

public sealed class BillingService(IWorkflowStore store, IConfirmedOrderSource orders, IPaymentEvidence payments,
    IRefundEvidence refunds, IHostEnvironment environment) : IBillingService
{
    public async Task<BillingDocumentResponse> IssueAsync(Guid orderId, CancellationToken ct)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing")) throw new WorkflowException(503, "simulated_billing_disabled");
        var id = WorkflowIds.Stable($"invoice:{orderId}");
        var document = await store.UpdateAsync<BillingDocument>("billing", id.ToString(), async current =>
        {
            if (current is not null) return current;
            var order = await orders.GetConfirmedAsync(orderId, ct);
            var payment = await payments.GetEvidenceAsync(order.PaymentId, ct);
            if (payment.Status != "Captured" || payment.OrderId != orderId || payment.Amount != order.Amount || payment.Currency != order.Currency) throw new WorkflowException(409, "invoice_payment_not_verified");
            return new(id, orderId, order.PaymentId, payment.Amount, payment.Currency, "Issued", true, $"DEMO-{id:N}", DateTimeOffset.UtcNow);
        }, ct);
        return Map(document);
    }

    public async Task<BillingDocumentResponse> GetAsync(Guid id, CancellationToken ct) =>
        Map(await store.ReadAsync<BillingDocument>("billing", id.ToString(), ct) ?? throw new WorkflowException(404, "document_not_found"));

    public async Task<CreditNoteResponse> CreditAsync(Guid originalId, CreditNoteRequest request, CancellationToken ct)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing")) throw new WorkflowException(503, "simulated_billing_disabled");
        var original = await GetAsync(originalId, ct);
        if (original.PaymentId != request.PaymentId) throw new WorkflowException(409, "refund_document_mismatch");
        var refund = await refunds.GetRefundAsync(request.PaymentId, request.RefundId, ct);
        if (refund.Amount <= 0 || refund.Amount > original.Amount) throw new WorkflowException(409, "invalid_credit_amount");
        var id = WorkflowIds.Stable($"credit:{originalId}:{refund.RefundId}");
        var note = await store.UpdateAsync<CreditNote>("billing", id.ToString(), current => Task.FromResult(current ??
            new(id, originalId, refund.RefundId, refund.Amount, original.Currency, "Issued", true, $"DEMO-NC-{id:N}", DateTimeOffset.UtcNow)), ct);
        return Map(note);
    }

    private static BillingDocumentResponse Map(BillingDocument document) => new(document.DocumentId, document.OrderId,
        document.PaymentId, document.Amount, document.Currency, document.Status, document.Simulated,
        document.DocumentNumber, document.IssuedAtUtc);

    private static CreditNoteResponse Map(CreditNote note) => new(note.DocumentId, note.OriginalDocumentId,
        note.RefundId, note.Amount, note.Currency, note.Status, note.Simulated, note.DocumentNumber, note.IssuedAtUtc);
}
