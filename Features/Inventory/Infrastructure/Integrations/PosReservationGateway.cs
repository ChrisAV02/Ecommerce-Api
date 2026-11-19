using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce_Api.Features.Inventory.Application.Contracts;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Request;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;
using Ecommerce_Api.Features.Inventory.Application.Exceptions;

namespace Ecommerce_Api.Features.Inventory.Infrastructure.Integrations;

public sealed class PosReservationGateway(HttpClient client, IConfiguration configuration) : IPosReservationGateway
{
    public async Task<ReservationResponse> CreateAsync(string storeCode, CreateReservationRequest request,
        string key, string correlation, CancellationToken ct)
    {
        if (request.OrderId == Guid.Empty || request.Items.Select(i => i.Sku.Trim().ToUpperInvariant()).Distinct().Count() != request.Items.Count)
            throw new PosGatewayException(400, "reservation.invalid_request");
        var result = await SendAsync(HttpMethod.Post, $"api/v1/tiendas/{Uri.EscapeDataString(storeCode.Trim().ToUpperInvariant())}/reservas",
            new { pedidoExternoId = request.OrderId.ToString(), items = request.Items.Select(i => new { sku = i.Sku.Trim().ToUpperInvariant(), cantidad = i.Quantity }) }, key, correlation, ct);
        // The idempotent POST response can be a stale snapshot. Obtain current state.
        return await GetAsync(result.ReservationId, correlation, ct);
    }

    public Task<ReservationResponse> GetAsync(Guid id, string correlation, CancellationToken ct) =>
        SendAsync(HttpMethod.Get, $"api/v1/reservas/{id}", null, null, correlation, ct);

    public Task<ReservationResponse> CancelAsync(Guid id, string reason, string key, string correlation, CancellationToken ct) =>
        SendAsync(HttpMethod.Post, $"api/v1/reservas/{id}/cancelar", new { motivo = reason }, key, correlation, ct);

    private async Task<ReservationResponse> SendAsync(HttpMethod method, string path, object? body, string? idempotency,
        string correlation, CancellationToken ct)
    {
        var apiKey = configuration["Pos:ApiKey"];
        if (client.BaseAddress is null || string.IsNullOrWhiteSpace(apiKey)) throw new PosGatewayException(503, "pos.not_configured");
        try
        {
            using var message = new HttpRequestMessage(method, path);
            message.Headers.Add("X-Api-Key", apiKey);
            message.Headers.TryAddWithoutValidation("X-Correlation-Id", correlation);
            if (idempotency is not null) message.Headers.Add("Idempotency-Key", idempotency);
            if (body is not null) message.Content = JsonContent.Create(body);
            using var response = await client.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode)
            {
                int status = (int)response.StatusCode;
                if (status is not (400 or 404 or 409 or 422 or 503)) status = 502;
                throw new PosGatewayException(status, "pos.reservation_failed");
            }
            var value = await response.Content.ReadFromJsonAsync<PosReservation>(ct) ?? throw new PosGatewayException(502, "pos.invalid_response");
            return new(value.ReservaId, value.TiendaCodigo, value.PedidoExternoId, value.Estado,
                value.FechaCreacion, value.FechaExpiracion, value.Items.Select(i => new ReservationItem(i.Sku, i.Cantidad)).ToArray());
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new PosGatewayException(504, "pos.timeout"); }
        catch (HttpRequestException) { throw new PosGatewayException(503, "pos.unavailable"); }
        catch (JsonException) { throw new PosGatewayException(502, "pos.invalid_response"); }
    }

    private sealed record PosItem(string Sku, int Cantidad);
    private sealed record PosReservation(Guid ReservaId, string TiendaCodigo, string PedidoExternoId, string Estado,
        DateTimeOffset FechaCreacion, DateTimeOffset FechaExpiracion, IReadOnlyList<PosItem> Items);
}
