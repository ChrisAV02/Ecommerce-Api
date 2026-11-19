using Ecommerce_Api.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce_Api.Shared.Presentation;

public sealed class WorkflowExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not WorkflowException exception) return;
        var problem = new ProblemDetails { Status = exception.Status, Title = exception.Code };
        problem.Extensions["code"] = exception.Code;
        problem.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
        context.Result = new ObjectResult(problem) { StatusCode = exception.Status };
        context.ExceptionHandled = true;
    }
}
