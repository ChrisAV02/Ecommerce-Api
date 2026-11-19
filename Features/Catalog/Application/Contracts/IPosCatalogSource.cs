namespace Ecommerce_Api.Features.Catalog.Application.Contracts;

public interface IPosCatalogSource
{
    Task<IReadOnlyList<CatalogSourceProduct>> ReadProductsAsync(string correlationId, CancellationToken ct);
    Task<IReadOnlyList<CatalogSourceStore>> ReadStoresAsync(string correlationId, CancellationToken ct);
    Task<IReadOnlyList<CatalogSourceAssortment>> ReadAssortmentsAsync(string storeCode, string correlationId, CancellationToken ct);
}
