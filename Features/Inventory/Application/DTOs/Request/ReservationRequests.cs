using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Features.Inventory.Application.Dtos.Request;

public sealed record ReservationItem([Required, StringLength(50)] string Sku, [Range(1, 1000000)] int Quantity);
public sealed record CreateReservationRequest(Guid OrderId, [Required, MinLength(1), MaxLength(100)] IReadOnlyList<ReservationItem> Items);
public sealed record CancelReservationRequest([Required, StringLength(1000)] string Reason);
