using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce_Api.Features.Catalog.Application.Contracts;

namespace Ecommerce_Api.Features.Catalog.Infrastructure.Integrations;

public sealed class PosCatalogSource(HttpClient client, IConfiguration configuration) : IPosCatalogSource
{
    public async Task<IReadOnlyList<CatalogSourceProduct>> ReadProductsAsync(string correlationId, CancellationToken ct) =>
        (await ReadPagesAsync<PosCatalogProduct>("api/v1/productos", correlationId, ct))
            .Select(product => new CatalogSourceProduct(product.Sku, product.Nombre, product.Descripcion, product.Categoria, product.Activo))
            .ToArray();

    public async Task<IReadOnlyList<CatalogSourceStore>> ReadStoresAsync(string correlationId, CancellationToken ct) =>
        (await ReadPagesAsync<PosCatalogStore>("api/v1/tiendas", correlationId, ct))
            .Select(store => new CatalogSourceStore(store.Codigo, store.Nombre, store.Direccion, store.Latitud, store.Longitud, store.Activa))
            .ToArray();

    public async Task<IReadOnlyList<CatalogSourceAssortment>> ReadAssortmentsAsync(string storeCode, string correlationId, CancellationToken ct) =>
        (await ReadPagesAsync<PosCatalogAssortment>($"api/v1/tiendas/{Uri.EscapeDataString(storeCode)}/inventario", correlationId, ct))
            .Select(item => new CatalogSourceAssortment(item.TiendaCodigo, item.Sku))
            .ToArray();

    private async Task<IReadOnlyList<T>> ReadPagesAsync<T>(string path, string correlationId, CancellationToken ct)
    {
        var baseUrl = configuration["Pos:BaseUrl"];
        var apiKey = configuration["Pos:ApiKey"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme is not ("http" or "https")
            || !string.IsNullOrEmpty(baseUri.UserInfo) || !string.IsNullOrEmpty(baseUri.Query) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Configure POS URL and credentials.");
        var root = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/");
        var items = new List<T>();
        int? total = null;
        for (var page = 1; page <= 10000; page++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(root, $"{path}?pagina={page}&tamanoPagina=100"));
            request.Headers.Add("X-Api-Key", apiKey);
            if (!string.IsNullOrWhiteSpace(correlationId)) request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            using var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PosCatalogPage<T>>(cancellationToken: ct);
            if (result?.Items is null || result.Items.Any(item => item is null) || result.Total < 0 || result.Pagina != page
                || result.TamanoPagina != 100 || result.Items.Count > 100 || total.HasValue && total != result.Total)
                throw new InvalidDataException("Invalid POS pagination.");
            total ??= result.Total;
            items.AddRange(result.Items);
            if (items.Count > total.Value) throw new InvalidDataException("Inconsistent source count.");
            if (items.Count == total.Value) return items;
            if (result.Items.Count == 0) throw new InvalidDataException("Incomplete source page.");
        }
        throw new InvalidDataException("Source page limit exceeded.");
    }
}
