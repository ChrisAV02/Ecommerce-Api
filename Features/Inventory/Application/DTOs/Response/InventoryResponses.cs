using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Inventory.Application.Dtos.Response;

public sealed record AlternativeItem(StoreResponse Store, AvailabilityResponse Availability);
public sealed record AlternativesResponse(IReadOnlyList<AlternativeItem> Items,
    IReadOnlyList<string> UnavailableStoreCodes, bool RequiresCustomerConfirmation);
