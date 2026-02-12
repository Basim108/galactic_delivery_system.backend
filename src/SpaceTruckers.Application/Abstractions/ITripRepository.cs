using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Abstractions;

public interface ITripRepository
{
    Task<Trip?> GetAsync(TripId tripId, CancellationToken cancellationToken);

    Task AddAsync(Trip trip, CancellationToken cancellationToken);

    Task UpdateAsync(Trip trip, int expectedVersion, CancellationToken cancellationToken);
}
