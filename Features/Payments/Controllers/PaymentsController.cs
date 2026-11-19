using Ecommerce_Api.Features.Payments.Application.Dtos.Request;
using Ecommerce_Api.Features.Payments.Application.Dtos.Response;
using Ecommerce_Api.Features.Payments.Application.Services;
using Ecommerce_Api.Shared.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Payments.Controllers;

[ApiController, Route("api/v1/payments"), Tags("Payments"), WorkflowKey, ServiceFilter(typeof(WorkflowExceptionFilter))]
public sealed class PaymentsController(IPaymentService payments) : ControllerBase
{
    [HttpPost]
    public Task<PaymentResponse> Pay(PaymentRequest request, CancellationToken ct) => payments.PayAsync(request, Request.Headers["Idempotency-Key"].ToString(), ct);
    [HttpGet("{id:guid}")]
    public Task<PaymentResponse> Get(Guid id, CancellationToken ct) => payments.GetAsync(id, ct);
    [HttpPost("{id:guid}/reconcile")]
    public Task<PaymentResponse> Reconcile(Guid id, CancellationToken ct) => payments.ReconcileAsync(id, ct);
    [HttpPost("{id:guid}/refunds")]
    public Task<RefundResponse> Refund(Guid id, RefundRequest request, CancellationToken ct) => payments.RefundAsync(id, request, Request.Headers["Idempotency-Key"].ToString(), ct);
}
