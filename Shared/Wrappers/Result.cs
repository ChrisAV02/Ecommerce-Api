namespace Ecommerce_Api.Shared.Wrappers;

public enum ErrorKind
{
    Validation,
    NotFound,
    Conflict,
    BusinessRule
}

public sealed record ApplicationError(string Code, string Message, ErrorKind Kind);

public sealed class Result<T>
{
    private Result(T? value, ApplicationError? error)
    {
        Value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;
    public T? Value { get; }
    public ApplicationError? Error { get; }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(ApplicationError error) => new(default, error);
}
