using MediatR;
using Microsoft.Extensions.Logging;

namespace SpaceTruckers.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var handlerName = typeof(TRequest).Name;

        logger.LogTrace("{HandlerName} starts", handlerName);

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            logger.LogTrace("{HandlerName} ends", handlerName);
        }
    }
}
