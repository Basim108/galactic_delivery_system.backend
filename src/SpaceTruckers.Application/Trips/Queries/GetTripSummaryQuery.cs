using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Application.Trips.Queries;

public sealed record GetTripSummaryQuery(TripId TripId) : IRequest<TripSummaryDto>;

public sealed class GetTripSummaryHandler(ITripRepository tripRepository) : IRequestHandler<GetTripSummaryQuery, TripSummaryDto>
{
    public async Task<TripSummaryDto> Handle(GetTripSummaryQuery request, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException($"Trip '{request.TripId}' was not found.");

        return TripSummaryMapper.From(trip);
    }
}
