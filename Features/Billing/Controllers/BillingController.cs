using Ecommerce_Api.Features.Billing.Application.Dtos.Request;
using Ecommerce_Api.Features.Billing.Application.Dtos.Response;
using Ecommerce_Api.Features.Billing.Application.Services;
using Ecommerce_Api.Shared.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Billing.Controllers;

[ApiController, Route("api/v1/billing/documents"), Tags("Billing"), WorkflowKey, ServiceFilter(typeof(WorkflowExceptionFilter))]
public sealed class BillingController(IBillingService billing) : ControllerBase
{
    [HttpPost]
    public Task<BillingDocumentResponse> Issue(BillingRequest request, CancellationToken ct) => billing.IssueAsync(request.OrderId, ct);
    [HttpGet("{id:guid}")]
    public Task<BillingDocumentResponse> Get(Guid id, CancellationToken ct) => billing.GetAsync(id, ct);
    [HttpPost("{id:guid}/credit-notes")]
    public Task<CreditNoteResponse> Credit(Guid id, CreditNoteRequest request, CancellationToken ct) => billing.CreditAsync(id, request, ct);
}
