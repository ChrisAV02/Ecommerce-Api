namespace Ecommerce_Api.Shared.Exceptions;

public sealed class WorkflowException(int status, string code) : Exception(code)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}
