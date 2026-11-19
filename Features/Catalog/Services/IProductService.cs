using Ecommerce_Api.Features.Catalog.DTOs.Responses;

namespace Ecommerce_Api.Features.Catalog.Services;

public interface IProductService
{
    Task<ProductListResponseDto> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken);
}
