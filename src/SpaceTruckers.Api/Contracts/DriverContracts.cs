using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Api.Contracts;

public sealed record CreateDriverRequest(string Name, ResourceStatus Status);

public sealed record CreateDriverResponse(Guid DriverId);
