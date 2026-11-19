namespace Ecommerce_Api.Features.Payments.Application.Dtos.Response;

public sealed record PaymentResponse(Guid PaymentId, Guid OrderId, Guid ReservationId, string Status,
    decimal Amount, string Currency, bool Simulated = true);
public sealed record RefundResponse(Guid RefundId, Guid PaymentId, decimal Amount, string Status = "Refunded", bool Simulated = true);
