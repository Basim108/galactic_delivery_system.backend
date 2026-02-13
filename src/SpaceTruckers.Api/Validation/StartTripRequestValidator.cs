using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class StartTripRequestValidator : AbstractValidator<StartTripRequest>
{
    public StartTripRequestValidator()
    {
        RuleFor(x => x.RequestId).MaximumLength(ValidationConstants.MAX_REQUEST_ID_LENGTH);
    }
}
