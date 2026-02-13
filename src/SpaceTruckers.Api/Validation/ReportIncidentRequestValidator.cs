using FluentValidation;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.Api.Validation;

public sealed class ReportIncidentRequestValidator : AbstractValidator<ReportIncidentRequest>
{
    public ReportIncidentRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty().MaximumLength(ValidationConstants.MAX_NAME_LENGTH);
    }
}
