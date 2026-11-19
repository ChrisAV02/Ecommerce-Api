using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;

namespace Ecommerce_Api.Features.Catalog.Application.Services;

public interface IProductService
{
    Task<ProductListResponseDto> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken);
}
