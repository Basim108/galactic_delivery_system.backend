using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ValidationConstants.MAX_NAME_LENGTH);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(ValidationConstants.MAX_TYPE_LENGTH);
        RuleFor(x => x.CargoCapacity).GreaterThanOrEqualTo(ValidationConstants.MIN_CARGO_CAPACITY);
    }
}
