namespace Ecommerce_Api.Features.Payments.Application.Dtos.Request;

public sealed record PaymentRequest(Guid OrderId, Guid ReservationId, string Scenario = "Approved");
public sealed record RefundRequest(decimal? Amount, string Reason);
