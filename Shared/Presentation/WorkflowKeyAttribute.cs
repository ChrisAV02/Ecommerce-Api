using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce_Api.Shared.Presentation;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class WorkflowKeyAttribute(bool requireOperator = false) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var header = requireOperator ? "X-Operator-Key" : "X-Internal-Key";
        var setting = requireOperator ? "Operator:KeyHash" : "Internal:KeyHash";
        var input = context.HttpContext.Request.Headers[header];
        byte[] expected;
        try { expected = Convert.FromHexString(config[setting] ?? ""); }
        catch (FormatException) { expected = []; }
        if (input.Count == 1 && input[0] is { Length: >= 32 and <= 512 } key && expected.Length == 32 &&
            CryptographicOperations.FixedTimeEquals(expected, SHA256.HashData(Encoding.UTF8.GetBytes(key)))) return;
        context.Result = new ObjectResult(new ProblemDetails { Status = 401, Title = "Credencial de servicio u operador requerida." }) { StatusCode = 401 };
    }
}
