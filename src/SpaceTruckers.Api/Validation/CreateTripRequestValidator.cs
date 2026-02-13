using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class CreateTripRequestValidator : AbstractValidator<CreateTripRequest>
{
    public CreateTripRequestValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.RouteId).NotEmpty();
        RuleFor(x => x.CargoRequirement).GreaterThanOrEqualTo(ValidationConstants.MIN_CARGO_REQUIREMENT);
    }
}
