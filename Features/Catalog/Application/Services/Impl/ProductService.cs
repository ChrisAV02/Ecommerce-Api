
using Ecommerce_Api.Features.Catalog.Application.Dtos.Response;
using Ecommerce_Api.Features.Catalog.Application.Services;
using Ecommerce_Api.Features.Catalog.Domain.Repositories;

namespace Ecommerce_Api.Features.Catalog.Application.Services.Impl;

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
