namespace Ecommerce_Api.Features.Inventory.Application.Exceptions;

public sealed class PosGatewayException(int statusCode, string code) : Exception(code)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}
