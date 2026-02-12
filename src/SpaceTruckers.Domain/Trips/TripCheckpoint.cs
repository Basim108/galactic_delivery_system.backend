using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Trips;

public sealed record TripCheckpoint(CheckpointId Id, int Sequence, string Name);
