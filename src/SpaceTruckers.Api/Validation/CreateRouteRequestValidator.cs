using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class CreateRouteRequestValidator : AbstractValidator<CreateRouteRequest>
{
    public CreateRouteRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ValidationConstants.MAX_NAME_LENGTH);
        RuleFor(x => x.Checkpoints).NotNull().NotEmpty();
        RuleForEach(x => x.Checkpoints).NotEmpty().MaximumLength(ValidationConstants.MAX_NAME_LENGTH);
    }
}
