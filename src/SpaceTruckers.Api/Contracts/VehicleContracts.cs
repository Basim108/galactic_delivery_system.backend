using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Api.Contracts;

public sealed record CreateVehicleRequest(string Name, string Type, int CargoCapacity, ResourceStatus Status);

public sealed record CreateVehicleResponse(Guid VehicleId);
