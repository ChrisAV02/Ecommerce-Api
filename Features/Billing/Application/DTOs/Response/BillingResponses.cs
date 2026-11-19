namespace Ecommerce_Api.Features.Billing.Application.Dtos.Response;

public sealed record BillingDocumentResponse(Guid DocumentId, Guid OrderId, Guid PaymentId, decimal Amount, string Currency,
    string Status, bool Simulated, string DocumentNumber, DateTimeOffset IssuedAtUtc);
public sealed record CreditNoteResponse(Guid DocumentId, Guid OriginalDocumentId, Guid RefundId, decimal Amount, string Currency,
    string Status, bool Simulated, string DocumentNumber, DateTimeOffset IssuedAtUtc);
