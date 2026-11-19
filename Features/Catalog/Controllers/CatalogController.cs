using Ecommerce_Api.Features.Catalog.Application.Dtos.Request;
using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;
using Ecommerce_Api.Features.Catalog.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Features.Catalog.Controllers;

[ApiController]
[Route("api/v1/catalog")]
[Tags("Catalog")]
public sealed class CatalogController(IProductService products) : ControllerBase
{
    [HttpGet("stores", Name = "Catalog_GetStores")]
    [EndpointDescription("Tiendas activas para recojo. Distancia aproximada en línea recta, no distancia de conducción.")]
    public async Task<ActionResult<IReadOnlyList<StoreResponse>>> Stores(
        [FromServices] ICatalogBrowsingService browsing, CancellationToken ct,
        [Range(-90, 90)] decimal? latitude = null, [Range(-180, 180)] decimal? longitude = null)
    {
        if (latitude.HasValue != longitude.HasValue) return Problem(statusCode: 400, title: "Provide latitude and longitude together.");
        return Ok(await browsing.StoresAsync(latitude, longitude, ct));
    }

    [HttpGet("categories", Name = "Catalog_GetCategories")]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> Categories(
        [FromServices] ICatalogBrowsingService browsing, CancellationToken ct) => Ok(await browsing.CategoriesAsync(ct));

    [HttpGet("stores/{storeCode}/products", Name = "Catalog_GetStoreProducts")]
    public async Task<ActionResult<StoreCatalogResponse>> StoreProducts(string storeCode,
        [FromServices] ICatalogBrowsingService browsing, CancellationToken ct,
        [Range(1, int.MaxValue)] int page = 1, [Range(1, 100)] int pageSize = 10,
        [StringLength(200)] string? search = null, Guid? categoryId = null,
        [Range(typeof(decimal), "0", "9999999999.99")] decimal? minPrice = null,
        [Range(typeof(decimal), "0", "9999999999.99")] decimal? maxPrice = null,
        [RegularExpression("^(name|priceAsc|priceDesc)$")] string sort = "name")
    {
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice) return Problem(statusCode: 400, title: "Invalid price range.");
        var result = await browsing.ProductsAsync(storeCode, page, pageSize, search, categoryId, minPrice, maxPrice, sort, ct);
        return result is null ? Problem(statusCode: 404, title: "Active pickup store not found.") : Ok(result);
    }

    [HttpGet("products", Name = "Catalog_GetProducts")]
    [EndpointSummary("Consultar productos del catálogo comercial")]
    [EndpointDescription("Consulta paginada de productos pertenecientes al catálogo comercial de Tiendas Mass. No representa disponibilidad física de stock confirmada.")]
    [ProducesResponseType<ProductListResponseDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<ActionResult<ProductListResponseDto>> GetProducts(
        [FromQuery] GetProductsRequestDto request, CancellationToken cancellationToken) =>
        Ok(await products.GetProductsAsync(request.Page, request.PageSize, cancellationToken));

    [HttpPost("synchronizations", Name = "Catalog_SynchronizePos")]
    [EndpointSummary("Sincronizar catálogo desde POS usando precios académicos explícitos")]
    [ProducesResponseType<CatalogSynchronizationResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<CatalogSynchronizationResponseDto>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CatalogSynchronizationResponseDto>> Synchronize(
        [FromHeader(Name = "X-Internal-Key")] string? internalKey,
        [FromServices] IConfiguration configuration, [FromServices] ICatalogSynchronizationService synchronization,
        CancellationToken cancellationToken)
    {
        var denied = AuthorizeSynchronization(configuration, internalKey);
        if (denied is not null) return denied;
        var result = await synchronization.SynchronizeAsync(HttpContext.TraceIdentifier, cancellationToken);
        return StatusCode(result.Status == "Succeeded" ? StatusCodes.Status200OK : StatusCodes.Status502BadGateway, result);
    }

    [HttpGet("synchronizations/{id:guid}", Name = "Catalog_GetSynchronization")]
    [ProducesResponseType<CatalogSynchronizationResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CatalogSynchronizationResponseDto>> GetSynchronization(Guid id,
        [FromHeader(Name = "X-Internal-Key")] string? internalKey,
        [FromServices] IConfiguration configuration, [FromServices] ICatalogSynchronizationService synchronization,
        CancellationToken cancellationToken)
    {
        var denied = AuthorizeSynchronization(configuration, internalKey);
        if (denied is not null) return denied;
        var result = await synchronization.FindAsync(id, cancellationToken);
        return result is null ? Problem(statusCode: 404, title: "Synchronization not found.") : Ok(result);
    }

    private ObjectResult? AuthorizeSynchronization(IConfiguration configuration, string? key)
    {
        var expectedHash = configuration["Catalog:Synchronization:InternalKeyHash"];
        if (string.IsNullOrWhiteSpace(expectedHash)) return Problem(statusCode: 503, title: "Internal synchronization credentials are not configured.");
        byte[] expected;
        try { expected = Convert.FromHexString(expectedHash); }
        catch (FormatException) { return Problem(statusCode: 503, title: "Internal synchronization credentials are not configured."); }
        if (expected.Length != 32) return Problem(statusCode: 503, title: "Internal synchronization credentials are not configured.");
        if (string.IsNullOrWhiteSpace(key) || key.Length is < 32 or > 512
            || !CryptographicOperations.FixedTimeEquals(expected, SHA256.HashData(Encoding.UTF8.GetBytes(key))))
            return Problem(statusCode: 401, title: "Valid internal credentials are required.");
        if ((configuration["Catalog:DataSource"] ?? "PostgreSQL") != "PostgreSQL")
            return Problem(statusCode: 409, title: "Synchronization requires Catalog:DataSource=PostgreSQL.");
        return null;
    }
}
