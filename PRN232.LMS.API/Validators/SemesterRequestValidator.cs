using FluentValidation;
using PRN232.LMS.API.RequestModels;

namespace PRN232.LMS.API.Validators;

/// <summary>FluentValidation rules for <see cref="SemesterRequest"/>, run by <c>FluentValidationFilter</c>.</summary>
public class SemesterRequestValidator : AbstractValidator<SemesterRequest>
{
    public SemesterRequestValidator()
    {
        // Report only the first failure per field, e.g. not both "required" and "must be after".
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SemesterName)
            .NotEmpty().WithMessage("SemesterName is required.")
            .MaximumLength(100).WithMessage("SemesterName must be at most 100 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.");
    }
}
