using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Application.Drivers;

public sealed record CreateDriverCommand(DriverId DriverId, string Name, ResourceStatus Status) : IRequest;

public sealed class CreateDriverHandler(IDriverRepository driverRepository) : IRequestHandler<CreateDriverCommand>
{
    public async Task Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = Driver.Create(request.DriverId, request.Name, request.Status);
        await driverRepository.AddAsync(driver, cancellationToken);
    }
}
