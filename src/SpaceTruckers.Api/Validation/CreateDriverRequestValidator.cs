using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class CreateDriverRequestValidator : AbstractValidator<CreateDriverRequest>
{
    public CreateDriverRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ValidationConstants.MAX_NAME_LENGTH);
    }
}
