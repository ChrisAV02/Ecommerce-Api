using Ecommerce_Api.Features.Catalog.Domain.Entities;

namespace Ecommerce_Api.Features.Catalog.Application.Contracts;

public interface ICatalogSynchronizationStore
{
    Task<CatalogSynchronizationRun> StartAsync(CancellationToken ct);
    Task<CatalogSynchronizationRun?> FindAsync(Guid id, CancellationToken ct);
    Task FailAsync(Guid id, string code, CancellationToken ct);
    Task ApplyAsync(Guid runId, CatalogSourceSnapshot source, IReadOnlyDictionary<string, decimal> prices, CancellationToken ct);
}
