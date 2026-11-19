namespace Ecommerce_Api.Features.Payments.Domain.Entities;

public sealed record PaymentState(Guid PaymentId, Guid OrderId, Guid ReservationId, string Status,
    decimal Amount, string Currency, bool Simulated = true);
public sealed record PaymentAttempt(string Fingerprint, PaymentState Response);
public sealed record RefundState(Guid RefundId, Guid PaymentId, decimal Amount, string Status = "Refunded", bool Simulated = true);
public sealed record RefundEntry(string Fingerprint, RefundState Response);
public sealed record PaymentWorkflow(PaymentState Current, Dictionary<string, PaymentAttempt> Attempts,
    Dictionary<string, RefundEntry> Refunds, DateTimeOffset CreatedAtUtc);
