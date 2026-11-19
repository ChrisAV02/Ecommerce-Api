using Ecommerce_Api.Features.Payments.Application.Dtos.Request;
using Ecommerce_Api.Features.Payments.Application.Dtos.Response;
using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Payments.Application.Services;

public interface IPaymentService
{
    Task<PaymentResponse> GetAsync(Guid id, CancellationToken ct);
    Task<PaymentResponse> PayAsync(PaymentRequest request, string key, CancellationToken ct);
    Task<PaymentResponse> ReconcileAsync(Guid id, CancellationToken ct);
    Task<OrderPaymentSource> CancelBeforePaymentAsync(Guid orderId, string reason, CancellationToken ct);
    Task<RefundResponse> RefundAsync(Guid id, RefundRequest request, string key, CancellationToken ct);
}
