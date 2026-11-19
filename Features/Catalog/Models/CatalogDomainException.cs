namespace Ecommerce_Api.Features.Catalog.Models;

public sealed class CatalogDomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
