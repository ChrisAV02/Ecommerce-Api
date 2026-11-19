using Ecommerce_Api.Features.Catalog.Domain.Entities;


namespace Ecommerce_Api.Features.Catalog.Domain.Repositories;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken cancellationToken);
}


