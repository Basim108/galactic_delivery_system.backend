using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class ReachCheckpointRequestValidator : AbstractValidator<ReachCheckpointRequest>
{
    public ReachCheckpointRequestValidator()
    {
        RuleFor(x => x.CheckpointName).NotEmpty().MaximumLength(ValidationConstants.MAX_NAME_LENGTH);
    }
}
