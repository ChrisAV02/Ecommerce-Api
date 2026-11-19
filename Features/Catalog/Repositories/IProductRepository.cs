using Ecommerce_Api.Features.Catalog.DTOs;
using Ecommerce_Api.Features.Catalog.Models;

namespace Ecommerce_Api.Features.Catalog.Repositories;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken cancellationToken);
}


