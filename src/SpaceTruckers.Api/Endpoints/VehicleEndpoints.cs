using FluentValidation;
using MediatR;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Api.Validation;
using SpaceTruckers.Application.Vehicles;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Api.Endpoints;

public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/vehicles")
            .WithTags("Vehicles");

        group.MapPost("/", CreateVehicleAsync)
            .WithName("CreateVehicle");

        return endpoints;
    }

    private static async Task<IResult> CreateVehicleAsync(
        CreateVehicleRequest request,
        IValidator<CreateVehicleRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var vehicleId = VehicleId.New();
        await mediator.Send(
            new CreateVehicleCommand(vehicleId, request.Name, request.Type, request.CargoCapacity, request.Status),
            cancellationToken);

        return Results.Created($"/api/vehicles/{vehicleId}", new CreateVehicleResponse(vehicleId.Value));
    }
}
