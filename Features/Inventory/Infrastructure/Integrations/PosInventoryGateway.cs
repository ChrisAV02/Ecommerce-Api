using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce_Api.Features.Inventory.Application.Contracts;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;
using Ecommerce_Api.Features.Inventory.Application.Exceptions;

namespace Ecommerce_Api.Features.Inventory.Infrastructure.Integrations;

public sealed class PosInventoryGateway(HttpClient client, IConfiguration configuration) : IPosInventoryGateway
{
    public async Task<AvailabilityResponse> CheckAsync(string storeCode, string sku, int quantity,
        string correlationId, CancellationToken cancellationToken)
    {
        var key = configuration["Pos:ApiKey"];
        if (string.IsNullOrWhiteSpace(key) || client.BaseAddress is null) throw new PosGatewayException(503, "pos.not_configured");
        storeCode = storeCode.Trim().ToUpperInvariant();
        sku = sku.Trim().ToUpperInvariant();
        try
        {
            // Stock alone does not imply that a product is commercially active.
            var product = await ReadAsync<ProductState>($"api/v1/productos/{Uri.EscapeDataString(sku)}", key, correlationId, cancellationToken);
            if (!product.Activo) throw new PosGatewayException(409, "pos.product_inactive");
            var stock = await ReadAsync<StockState>($"api/v1/tiendas/{Uri.EscapeDataString(storeCode)}/inventario/{Uri.EscapeDataString(sku)}", key, correlationId, cancellationToken);
            if (!string.Equals(stock.CodigoTienda, storeCode, StringComparison.Ordinal) ||
                !string.Equals(stock.Sku, sku, StringComparison.Ordinal) || stock.StockDisponible < 0 || stock.FechaConsulta == default)
                throw new PosGatewayException(502, "pos.invalid_response");
            return new(storeCode, sku, quantity, stock.StockDisponible, stock.StockDisponible >= quantity, stock.FechaConsulta);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PosGatewayException(504, "pos.timeout");
        }
        catch (HttpRequestException) { throw new PosGatewayException(503, "pos.unavailable"); }
        catch (JsonException) { throw new PosGatewayException(502, "pos.invalid_response"); }
    }

    private async Task<T> ReadAsync<T>(string path, string key, string correlationId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("X-Api-Key", key);
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var status = response.StatusCode switch
            {
                HttpStatusCode.NotFound => 404,
                HttpStatusCode.ServiceUnavailable => 503,
                HttpStatusCode.TooManyRequests => 503,
                _ => 502
            };
            throw new PosGatewayException(status, status == 404 ? "pos.resource_not_found" : "pos.upstream_failure");
        }
        return await response.Content.ReadFromJsonAsync<T>(ct) ?? throw new PosGatewayException(502, "pos.invalid_response");
    }

    private sealed record ProductState(bool Activo);
    private sealed record StockState(string CodigoTienda, string Sku, int StockDisponible, DateTimeOffset FechaConsulta);
}
