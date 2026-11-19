using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Api.Features.Catalog.Application.Dtos.Request;

public sealed class GetProductsRequestDto
{
    [Range(1, int.MaxValue), DefaultValue(1)]
    public int Page { get; init; } = 1;

    [Range(1, 100), DefaultValue(10)]
    public int PageSize { get; init; } = 10;
}
