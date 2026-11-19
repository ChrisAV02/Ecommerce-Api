namespace Ecommerce_Api.Features.Catalog.Domain.Repositories;

// Internal repository pagination types; these are not HTTP response schemas.
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);

public sealed record ProductQuery(int Page = 1, int PageSize = 10);
