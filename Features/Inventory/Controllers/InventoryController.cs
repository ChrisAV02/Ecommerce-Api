using System.ComponentModel.DataAnnotations;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;
using Ecommerce_Api.Features.Inventory.Application.Exceptions;
using Ecommerce_Api.Features.Inventory.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Inventory.Controllers;

[ApiController, Route("api/v1/inventory"), Tags("Inventory")]
public sealed class InventoryController(IInventoryService inventory) : ControllerBase
{
    [HttpGet("products/{sku}/alternatives", Name = "Inventory_GetAlternatives")]
    [EndpointDescription("Hasta tres tiendas cercanas con disponibilidad consultada al POS; requiere confirmación del cliente para cambiar de tienda.")]
    public async Task<ActionResult<AlternativesResponse>> Alternatives(
        [StringLength(50, MinimumLength = 1), RegularExpression("^[A-Za-z0-9._-]+$")] string sku,
        CancellationToken ct,
        [Required, Range(-90, 90)] decimal? latitude,
        [Required, Range(-180, 180)] decimal? longitude,
        [Range(1, 1000000)] int quantity = 1, [StringLength(16)] string? excludedStoreCode = null) =>
        Ok(await inventory.GetAlternativesAsync(sku, latitude, longitude, quantity, excludedStoreCode, HttpContext.TraceIdentifier, ct));

    [HttpGet("stores/{storeCode}/products/{sku}/availability", Name = "Inventory_CheckAvailability")]
    [EndpointDescription("Consulta puntual autenticada al POS. No reserva unidades ni agrega productos al carrito.")]
    [ProducesResponseType<AvailabilityResponse>(200)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(409)]
    [ProducesResponseType<ProblemDetails>(502)]
    [ProducesResponseType<ProblemDetails>(503)]
    [ProducesResponseType<ProblemDetails>(504)]
    public async Task<ActionResult<AvailabilityResponse>> Check(
        [StringLength(16, MinimumLength = 1), RegularExpression("^[A-Za-z0-9._-]+$")] string storeCode,
        [StringLength(50, MinimumLength = 1), RegularExpression("^[A-Za-z0-9._-]+$")] string sku,
        CancellationToken cancellationToken, [Range(1, 1000000)] int quantity = 1)
    {
        var correlationId = HttpContext.TraceIdentifier;
        Response.Headers["X-Correlation-Id"] = correlationId;
        try { return Ok(await inventory.CheckAvailabilityAsync(storeCode, sku, quantity, correlationId, cancellationToken)); }
        catch (PosGatewayException exception)
        {
            return Problem(statusCode: exception.StatusCode, title: "No se pudo verificar disponibilidad en POS.",
                extensions: new Dictionary<string, object?> { ["code"] = exception.Code, ["correlationId"] = correlationId });
        }
    }
}
