using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Routes;

public sealed record Route(RouteId Id, string Name, IReadOnlyList<RouteCheckpoint> Checkpoints)
{
    public static Route Create(RouteId id, string name, IReadOnlyList<string> checkpointNames)
    {
        if (checkpointNames.Count == 0)
        {
            throw new ArgumentException("Route must have at least one checkpoint.", nameof(checkpointNames));
        }

        var checkpoints = new List<RouteCheckpoint>(checkpointNames.Count);
        for (var i = 0; i < checkpointNames.Count; i++)
        {
            var checkpointName = checkpointNames[i].Trim();
            if (string.IsNullOrWhiteSpace(checkpointName))
            {
                throw new ArgumentException("Checkpoint name cannot be empty.", nameof(checkpointNames));
            }

            checkpoints.Add(RouteCheckpoint.Create(i + 1, checkpointName));
        }

        return new Route(id, name, checkpoints);
    }
}
