using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_Api.Features.Catalog.DTOs.Requests;

public sealed class GetProductsRequestDto
{
    [FromQuery(Name = "page"), Range(1, int.MaxValue), DefaultValue(1)]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize"), Range(1, 100), DefaultValue(10)]
    public int PageSize { get; init; } = 10;
}
