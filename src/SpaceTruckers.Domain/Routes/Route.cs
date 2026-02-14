using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Routes;

public sealed class Route
{
    private readonly List<RouteCheckpoint> _checkpoints = new();

    // For EF Core.
    private Route() { }

    private Route(RouteId id, string name, IReadOnlyList<RouteCheckpoint> checkpoints)
    {
        if (checkpoints.Count == 0)
        {
            throw new ArgumentException("Route must have at least one checkpoint.", nameof(checkpoints));
        }

        Id = id;
        Name = name;
        _checkpoints.AddRange(checkpoints);
    }

    public RouteId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public IReadOnlyList<RouteCheckpoint> Checkpoints => _checkpoints;

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
