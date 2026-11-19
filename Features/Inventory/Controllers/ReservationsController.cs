using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Request;
using Ecommerce_Api.Features.Inventory.Application.Dtos.Response;
using Ecommerce_Api.Features.Inventory.Application.Exceptions;
using Ecommerce_Api.Features.Inventory.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Inventory.Controllers;

[ApiController, Route("api/v1/inventory"), Tags("Inventory")]
public sealed class ReservationsController(IInventoryService inventory, IConfiguration configuration) : ControllerBase
{
    [HttpPost("stores/{storeCode}/reservations", Name = "Inventory_CreateReservation")]
    public Task<ActionResult<ReservationResponse>> Create(
        [StringLength(16, MinimumLength = 1), RegularExpression("^[A-Za-z0-9._-]+$")] string storeCode,
        CreateReservationRequest request, [FromHeader(Name = "Idempotency-Key"), Required, StringLength(100)] string key,
        CancellationToken ct) => Execute(() => inventory.CreateReservationAsync(storeCode, request, key, HttpContext.TraceIdentifier, ct));

    [HttpGet("reservations/{id:guid}", Name = "Inventory_GetReservation")]
    public Task<ActionResult<ReservationResponse>> Get(Guid id, CancellationToken ct) =>
        Execute(() => inventory.GetReservationAsync(id, HttpContext.TraceIdentifier, ct));

    [HttpPost("reservations/{id:guid}/cancel", Name = "Inventory_CancelReservation")]
    public Task<ActionResult<ReservationResponse>> Cancel(Guid id, CancelReservationRequest request,
        [FromHeader(Name = "Idempotency-Key"), Required, StringLength(100)] string key, CancellationToken ct) =>
        Execute(() => inventory.CancelReservationAsync(id, request, key, HttpContext.TraceIdentifier, ct));

    private async Task<ActionResult<ReservationResponse>> Execute(Func<Task<ReservationResponse>> action)
    {
        var hash = configuration["Internal:KeyHash"];
        if (string.IsNullOrWhiteSpace(hash)) return Problem(statusCode: 503, title: "Internal authorization is not configured.");
        var key = Request.Headers["X-Internal-Key"];
        if (key.Count != 1 || key[0] is not { Length: >= 32 and <= 512 }) return Unauthorized();
        try
        {
            var expected = Convert.FromHexString(hash);
            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(key[0]!));
            if (expected.Length != actual.Length || !CryptographicOperations.FixedTimeEquals(expected, actual)) return Unauthorized();
        }
        catch (FormatException) { return Problem(statusCode: 503, title: "Invalid authorization configuration."); }
        Response.Headers["X-Correlation-Id"] = HttpContext.TraceIdentifier;
        try { return Ok(await action()); }
        catch (PosGatewayException exception)
        {
            return Problem(statusCode: exception.StatusCode, title: "POS reservation could not be processed.",
                extensions: new Dictionary<string, object?> { ["code"] = exception.Code, ["correlationId"] = HttpContext.TraceIdentifier });
        }
    }
}
