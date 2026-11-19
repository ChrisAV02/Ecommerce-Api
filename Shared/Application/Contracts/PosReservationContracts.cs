namespace Ecommerce_Api.Shared.Application.Contracts;

public sealed record PosReservationLine(string Sku, int Cantidad);
public sealed record PosReservation(Guid ReservaId, string TiendaCodigo, string PedidoExternoId, string Estado,
    DateTimeOffset FechaExpiracion, IReadOnlyList<PosReservationLine> Items, string? ReferenciaAutorizacionPago = null);

public interface IPosReservationClient
{
    Task<PosReservation> GetAsync(Guid id, CancellationToken ct);
    Task<PosReservation> ConfirmAsync(Guid id, Guid paymentId, CancellationToken ct);
}
