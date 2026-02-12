using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class CreateDriverRequestValidator : AbstractValidator<CreateDriverRequest>
{
    public CreateDriverRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CargoCapacity).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateRouteRequestValidator : AbstractValidator<CreateRouteRequest>
{
    public CreateRouteRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Checkpoints).NotNull().NotEmpty();
        RuleForEach(x => x.Checkpoints).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateTripRequestValidator : AbstractValidator<CreateTripRequest>
{
    public CreateTripRequestValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.RouteId).NotEmpty();
        RuleFor(x => x.CargoRequirement).GreaterThanOrEqualTo(0);
    }
}

public sealed class StartTripRequestValidator : AbstractValidator<StartTripRequest>
{
    public StartTripRequestValidator()
    {
        RuleFor(x => x.RequestId).MaximumLength(100);
    }
}

public sealed class ReachCheckpointRequestValidator : AbstractValidator<ReachCheckpointRequest>
{
    public ReachCheckpointRequestValidator()
    {
        RuleFor(x => x.CheckpointName).NotEmpty().MaximumLength(200);
    }
}

public sealed class ReportIncidentRequestValidator : AbstractValidator<ReportIncidentRequest>
{
    public ReportIncidentRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty().MaximumLength(200);
    }
}
