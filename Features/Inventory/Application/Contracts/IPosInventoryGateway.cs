using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Inventory.Application.Contracts;

public interface IPosInventoryGateway
{
    Task<AvailabilityResponse> CheckAsync(string storeCode, string sku, int quantity, string correlationId, CancellationToken cancellationToken);
}
