using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Application.Abstractions;

public interface IDriverRepository
{
    Task<Driver?> GetAsync(DriverId driverId, CancellationToken cancellationToken);

    Task AddAsync(Driver driver, CancellationToken cancellationToken);
}
