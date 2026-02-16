using FluentValidation;
using MediatR;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Api.Validation;
using SpaceTruckers.Application.Drivers;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Api.Endpoints;

public static class DriverEndpoints
{
    private sealed class DriverEndpoint;
    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/drivers")
            .WithTags("Drivers");

        group.MapPost("/", CreateDriverAsync)
            .WithName("CreateDriver");

        return endpoints;
    }

    private static async Task<IResult> CreateDriverAsync(
        CreateDriverRequest request,
        IValidator<CreateDriverRequest> validator,
        IMediator mediator,
        ILogger<DriverEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var driverId = DriverId.New();

        using var driverIdScope = logger.BeginScope("DriverId={DriverId}", driverId.Value);

        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        await mediator.Send(new CreateDriverCommand(driverId, request.Name, request.Status), cancellationToken);

        return Results.Created($"/api/drivers/{driverId}", new CreateDriverResponse(driverId.Value));
    }
}
