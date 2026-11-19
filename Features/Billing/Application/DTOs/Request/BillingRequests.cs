namespace Ecommerce_Api.Features.Billing.Application.Dtos.Request;

public sealed record BillingRequest(Guid OrderId);
public sealed record CreditNoteRequest(Guid PaymentId, Guid RefundId);
