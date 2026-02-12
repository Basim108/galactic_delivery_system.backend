using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Routes;

public sealed record RouteCheckpoint(CheckpointId Id, int Sequence, string Name)
{
    public static RouteCheckpoint Create(int sequence, string name) => new(CheckpointId.New(), sequence, name);
}
