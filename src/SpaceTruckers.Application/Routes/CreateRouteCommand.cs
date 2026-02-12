using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.Application.Routes;

public sealed record CreateRouteCommand(RouteId RouteId, string Name, IReadOnlyList<string> Checkpoints) : IRequest;

public sealed class CreateRouteHandler(IRouteRepository routeRepository) : IRequestHandler<CreateRouteCommand>
{
    public async Task Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = Route.Create(request.RouteId, request.Name, request.Checkpoints);
        await routeRepository.AddAsync(route, cancellationToken);
    }
}
