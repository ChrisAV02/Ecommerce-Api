using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Catalog.Application.Services;

public interface ICatalogSynchronizationService
{
    Task<CatalogSynchronizationResponseDto> SynchronizeAsync(string correlationId, CancellationToken ct);
    Task<CatalogSynchronizationResponseDto?> FindAsync(Guid id, CancellationToken ct);
}
