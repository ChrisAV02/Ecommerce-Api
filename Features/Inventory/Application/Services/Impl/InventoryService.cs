using Ecommerce_Api.Features.Catalog.Application.Services;
using Ecommerce_Api.Features.Inventory.Application.Contracts;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Request;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;
using Ecommerce_Api.Features.Inventory.Application.Exceptions;

namespace Ecommerce_Api.Features.Inventory.Application.Services.Impl;

public sealed class InventoryService(IPosInventoryGateway inventory, IPosReservationGateway reservations,
    ICatalogBrowsingService catalog) : IInventoryService
{
    public Task<AvailabilityResponse> CheckAvailabilityAsync(string storeCode, string sku, int quantity,
        string correlationId, CancellationToken ct) => inventory.CheckAsync(storeCode, sku, quantity, correlationId, ct);

    public async Task<AlternativesResponse> GetAlternativesAsync(string sku, decimal? latitude, decimal? longitude,
        int quantity, string? excludedStoreCode, string correlationId, CancellationToken ct)
    {
        var stores = await catalog.StoresAsync(latitude, longitude, ct);
        var alternatives = new List<AlternativeItem>();
        var unavailable = new List<string>();
        foreach (var store in stores.Where(s => !string.Equals(s.StoreCode, excludedStoreCode?.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var result = await inventory.CheckAsync(store.StoreCode, sku, quantity, correlationId, ct);
                if (!result.Sufficient) continue;
                alternatives.Add(new(store, result));
                if (alternatives.Count == 3) break;
            }
            catch (PosGatewayException exception) when (exception.StatusCode is 404 or 409 or 503 or 504)
            {
                unavailable.Add(store.StoreCode);
            }
        }
        return new(alternatives, unavailable, true);
    }

    public Task<ReservationResponse> CreateReservationAsync(string storeCode, CreateReservationRequest request,
        string key, string correlationId, CancellationToken ct) => reservations.CreateAsync(storeCode, request, key, correlationId, ct);

    public Task<ReservationResponse> GetReservationAsync(Guid id, string correlationId, CancellationToken ct) =>
        reservations.GetAsync(id, correlationId, ct);

    public Task<ReservationResponse> CancelReservationAsync(Guid id, CancelReservationRequest request,
        string key, string correlationId, CancellationToken ct) => reservations.CancelAsync(id, request.Reason, key, correlationId, ct);
}
