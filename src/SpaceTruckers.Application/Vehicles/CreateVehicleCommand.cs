using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Application.Vehicles;

public sealed record CreateVehicleCommand(
    VehicleId VehicleId,
    string Name,
    string Type,
    int CargoCapacity,
    ResourceStatus Status) : IRequest;

public sealed class CreateVehicleHandler(IVehicleRepository vehicleRepository) : IRequestHandler<CreateVehicleCommand>
{
    public async Task Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = Vehicle.Create(
            request.VehicleId,
            request.Name,
            request.Type,
            CargoCapacity.From(request.CargoCapacity),
            request.Status);

        await vehicleRepository.AddAsync(vehicle, cancellationToken);
    }
}
