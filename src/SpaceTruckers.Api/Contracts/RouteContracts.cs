namespace SpaceTruckers.Api.Contracts;

public sealed record CreateRouteRequest(string Name, IReadOnlyList<string> Checkpoints);

public sealed record CreateRouteResponse(Guid RouteId);
