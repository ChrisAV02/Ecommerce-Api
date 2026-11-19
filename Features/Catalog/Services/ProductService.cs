using Ecommerce_Api.Features.Catalog.DTOs;
using Ecommerce_Api.Features.Catalog.DTOs.Responses;
using Ecommerce_Api.Features.Catalog.Repositories;

namespace Ecommerce_Api.Features.Catalog.Services;

public sealed class ProductService(IProductRepository products) : IProductService
{
    public async Task<ProductListResponseDto> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

        var result = await products.GetPagedAsync(new ProductQuery(page, pageSize), cancellationToken);
        return new(result.Items.Select(product => new ProductResponseDto(
            product.Id, product.Sku.Value, product.Name, product.Description,
            new(product.BasePrice.Amount, product.BasePrice.Currency), product.IsActive)).ToArray(),
            page, pageSize, result.TotalCount);
    }
}
