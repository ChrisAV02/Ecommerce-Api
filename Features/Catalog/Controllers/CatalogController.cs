using Ecommerce_Api.Features.Catalog.DTOs.Requests;
using Ecommerce_Api.Features.Catalog.DTOs.Responses;
using Ecommerce_Api.Features.Catalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Catalog.Controllers;

[ApiController]
[Route("api/v1/catalog")]
[Tags("Catalog")]
public sealed class CatalogController(IProductService products) : ControllerBase
{
    [HttpGet("products", Name = "Catalog_GetProducts")]
    [EndpointSummary("Consultar productos del catálogo comercial")]
    [EndpointDescription("Consulta paginada de productos pertenecientes al catálogo comercial de Tiendas Mass. No representa disponibilidad física de stock confirmada.")]
    [ProducesResponseType<ProductListResponseDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<ActionResult<ProductListResponseDto>> GetProducts(
        [FromQuery] GetProductsRequestDto request, CancellationToken cancellationToken) =>
        Ok(await products.GetProductsAsync(request.Page, request.PageSize, cancellationToken));
}
