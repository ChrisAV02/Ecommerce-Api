using System.Security.Cryptography;
using System.Text;
using Ecommerce_Api.Shared.Exceptions;

namespace Ecommerce_Api.Shared.Application;

public static class WorkflowRules
{
    public static string RequiredKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100) throw new WorkflowException(400, "idempotency_key_required");
        return key;
    }

    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
