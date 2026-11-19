namespace Ecommerce_Api.Shared.Application;

public interface IWorkflowStore
{
    Task<T?> ReadAsync<T>(string schema, string key, CancellationToken ct);
    Task<T> UpdateAsync<T>(string schema, string key, Func<T?, Task<T>> update, CancellationToken ct);
}
