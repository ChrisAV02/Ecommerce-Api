using Ecommerce_Api.Features.Inventory.Application.Dtos.Request;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Inventory.Application.Contracts;

public interface IPosReservationGateway
{
    Task<ReservationResponse> CreateAsync(string storeCode, CreateReservationRequest request, string key, string correlation, CancellationToken ct);
    Task<ReservationResponse> GetAsync(Guid id, string correlation, CancellationToken ct);
    Task<ReservationResponse> CancelAsync(Guid id, string reason, string key, string correlation, CancellationToken ct);
}
