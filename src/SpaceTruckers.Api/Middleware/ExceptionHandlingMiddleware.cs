using System.Net;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Exceptions;

namespace SpaceTruckers.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainRuleViolationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, ex.Message, ex.Code);
        }
        catch (OptimisticConcurrencyException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, ex.Message, "OptimisticConcurrencyException");
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ex.Message, "NotFound");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string detail, string code)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "about:blank",
            title = statusCode.ToString(),
            status = (int)statusCode,
            detail,
            code,
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
