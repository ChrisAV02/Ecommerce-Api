using Ecommerce_Api.Features.Billing.Application.Dtos.Request;
using Ecommerce_Api.Features.Billing.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Billing.Application.Services;

public interface IBillingService
{
    Task<BillingDocumentResponse> IssueAsync(Guid orderId, CancellationToken ct);
    Task<BillingDocumentResponse> GetAsync(Guid id, CancellationToken ct);
    Task<CreditNoteResponse> CreditAsync(Guid originalId, CreditNoteRequest request, CancellationToken ct);
}
