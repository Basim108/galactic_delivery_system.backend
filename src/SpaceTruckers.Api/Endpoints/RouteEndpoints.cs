using FluentValidation;
using MediatR;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Api.Validation;
using SpaceTruckers.Application.Routes;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Api.Endpoints;

public static class RouteEndpoints
{
    public static IEndpointRouteBuilder MapRouteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/routes")
            .WithTags("Routes");

        group.MapPost("/", CreateRouteAsync)
            .WithName("CreateRoute");

        return endpoints;
    }

    private static async Task<IResult> CreateRouteAsync(
        CreateRouteRequest request,
        IValidator<CreateRouteRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var routeId = RouteId.New();
        await mediator.Send(new CreateRouteCommand(routeId, request.Name, request.Checkpoints), cancellationToken);

        return Results.Created($"/api/routes/{routeId}", new CreateRouteResponse(routeId.Value));
    }
}
