using System.Net.Http.Json;
using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Infrastructure.Integrations;

public sealed class PosReservationClient(HttpClient client, IConfiguration configuration) : IPosReservationClient
{
    public async Task<PosReservation> GetAsync(Guid id, CancellationToken ct) =>
        await SendAsync(HttpMethod.Get, $"api/v1/reservas/{id}", null, null, ct);

    public async Task<PosReservation> ConfirmAsync(Guid id, Guid paymentId, CancellationToken ct) =>
        await SendAsync(HttpMethod.Post, $"api/v1/reservas/{id}/confirmar",
            new { referenciaAutorizacionPago = $"SIMULATED-CAPTURE-{paymentId}" }, $"order-confirm-{paymentId}", ct);

    private async Task<PosReservation> SendAsync(HttpMethod method, string path, object? body, string? idempotency, CancellationToken ct)
    {
        if (!Uri.TryCreate(configuration["Pos:BaseUrl"], UriKind.Absolute, out var root)) throw new WorkflowException(503, "pos_not_configured");
        var key = configuration["Pos:IntegrationApiKey"] ?? configuration["Pos:ApiKey"];
        if (string.IsNullOrWhiteSpace(key)) throw new WorkflowException(503, "pos_not_configured");
        using var request = new HttpRequestMessage(method, new Uri(new Uri(root.ToString().TrimEnd('/') + "/"), path));
        request.Headers.Add("X-Api-Key", key);
        request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());
        if (idempotency is not null) request.Headers.Add("Idempotency-Key", idempotency);
        if (body is not null) request.Content = JsonContent.Create(body);
        try
        {
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) throw new WorkflowException((int)response.StatusCode is 404 or 409 or 422 ? 409 : 503, "pos_reservation_unavailable");
            return await response.Content.ReadFromJsonAsync<PosReservation>(ct) ?? throw new WorkflowException(502, "pos_invalid_response");
        }
        catch (HttpRequestException) { throw new WorkflowException(503, "pos_unavailable"); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new WorkflowException(504, "pos_timeout"); }
    }
}
