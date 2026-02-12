using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Resources;

public sealed record Driver(DriverId Id, string Name, ResourceStatus Status)
{
    public static Driver Create(DriverId id, string name, ResourceStatus status) => new(id, name, status);
}
