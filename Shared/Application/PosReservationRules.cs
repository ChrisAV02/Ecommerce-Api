using Ecommerce_Api.Shared.Application.Contracts;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Shared.Application;

public static class PosReservationRules
{
    public static void VerifyMatches(PosReservation reservation, OrderPaymentSource order)
    {
        if (reservation.TiendaCodigo != order.StoreCode || reservation.PedidoExternoId != order.OrderId.ToString() ||
            reservation.Items.Count != order.Items.Count || reservation.Items.Any(item =>
                !order.Items.Any(line => line.Sku == item.Sku && line.Quantity == item.Cantidad)))
            throw new WorkflowException(409, "reservation_order_mismatch");
    }
}
