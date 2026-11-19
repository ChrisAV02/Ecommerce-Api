using Ecommerce_Api.Features.Sales.Domain.Entities;
using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Sales.Application.Dtos.Response;

public sealed record OrderResponse(Guid OrderId, Guid CustomerId, Guid StoreId, string StoreCode,
    IReadOnlyList<QuotedLine> Items, decimal TotalAmount, string Currency, string Status,
    Guid? PaymentId, Guid? ReservationId, IReadOnlyDictionary<string, int> Prepared,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? CollectedAtUtc, IReadOnlyList<OrderTransition> History)
{
    public static OrderResponse From(OrderWorkflow order) => new(order.OrderId, order.CustomerId, order.StoreId, order.StoreCode,
        order.Items, order.TotalAmount, order.Currency, order.Status, order.PaymentId, order.ReservationId,
        order.Prepared, order.CreatedAtUtc, order.CollectedAtUtc, order.History);
}
