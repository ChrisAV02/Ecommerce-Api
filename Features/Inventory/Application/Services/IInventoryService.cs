using Ecommerce_Api.Features.Inventory.Application.Dtos.Request;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Inventory.Application.Services;

public interface IInventoryService
{
    Task<AlternativesResponse> GetAlternativesAsync(string sku, decimal? latitude, decimal? longitude,
        int quantity, string? excludedStoreCode, string correlationId, CancellationToken ct);
    Task<AvailabilityResponse> CheckAvailabilityAsync(string storeCode, string sku, int quantity,
        string correlationId, CancellationToken ct);
    Task<ReservationResponse> CreateReservationAsync(string storeCode, CreateReservationRequest request,
        string key, string correlationId, CancellationToken ct);
    Task<ReservationResponse> GetReservationAsync(Guid id, string correlationId, CancellationToken ct);
    Task<ReservationResponse> CancelReservationAsync(Guid id, CancelReservationRequest request,
        string key, string correlationId, CancellationToken ct);
}
