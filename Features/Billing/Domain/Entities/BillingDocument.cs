namespace Ecommerce_Api.Features.Billing.Domain.Entities;

public sealed record BillingDocument(Guid DocumentId, Guid OrderId, Guid PaymentId, decimal Amount, string Currency,
    string Status, bool Simulated, string DocumentNumber, DateTimeOffset IssuedAtUtc);
public sealed record CreditNote(Guid DocumentId, Guid OriginalDocumentId, Guid RefundId, decimal Amount, string Currency,
    string Status, bool Simulated, string DocumentNumber, DateTimeOffset IssuedAtUtc);
