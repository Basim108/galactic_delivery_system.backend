using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Application.Trips.Queries;

public sealed record GetTripQuery(TripId TripId) : IRequest<TripDto>;

public sealed class GetTripHandler(ITripRepository tripRepository) : IRequestHandler<GetTripQuery, TripDto>
{
    public async Task<TripDto> Handle(GetTripQuery request, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException($"Trip '{request.TripId}' was not found.");

        return TripMapper.ToDto(trip);
    }
}
