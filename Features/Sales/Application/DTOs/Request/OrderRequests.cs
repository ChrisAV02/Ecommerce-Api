using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Sales.Application.Dtos.Request;

public sealed record CreateOrderRequest(Guid CustomerId, string StoreCode, IReadOnlyList<QuoteLine> Items);
public sealed record ConfirmOrderRequest(Guid PaymentId, Guid ReservationId);
public sealed record PickingRequest(string Sku, int FoundQuantity, string OperatorId);
public sealed record PickupRequest(string StoreCode, string Code, string OperatorId);
public sealed record OperatorRequest(string OperatorId);
public sealed record CancelOrderRequest(string Reason);
