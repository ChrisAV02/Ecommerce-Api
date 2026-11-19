using Ecommerce_Api.Shared.Application.Contracts;

namespace Ecommerce_Api.Features.Sales.Domain.Entities;

public sealed record OrderWorkflow(Guid OrderId, Guid CustomerId, Guid StoreId, string StoreCode,
    IReadOnlyList<QuotedLine> Items, decimal TotalAmount, string Currency, string Status,
    string Fingerprint, Guid? PaymentId, Guid? ReservationId, Dictionary<string, int> Prepared,
    string? PickupCodeHash, DateTimeOffset CreatedAtUtc, DateTimeOffset? CollectedAtUtc, List<OrderTransition> History);
public sealed record OrderTransition(string Status, string Actor, DateTimeOffset AtUtc);
