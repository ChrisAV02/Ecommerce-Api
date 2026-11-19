namespace Ecommerce_Api.Features.Catalog.Domain.Exceptions;

public sealed class CatalogDomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
