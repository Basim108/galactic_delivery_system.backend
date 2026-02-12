using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Resources;

public sealed record Vehicle(VehicleId Id, string Name, string Type, CargoCapacity CargoCapacity, ResourceStatus Status)
{
    public static Vehicle Create(VehicleId id, string name, string type, CargoCapacity cargoCapacity, ResourceStatus status) =>
        new(id, name, type, cargoCapacity, status);
}
