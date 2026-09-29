using FluentValidation;
using StudentManager.Api.Dtos;

namespace StudentManager.Api.Validation;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    // Digits, spaces, dashes and brackets, optional leading +, 7-20 characters in total.
    // The React form uses the same pattern (frontend/src/validation.ts).
    private const string PhonePattern = @"^\+?[0-9][0-9 \-()]{6,19}$";
    private const int MaxAgeYears = 120;

    public CreateStudentRequestValidator(TimeProvider clock)
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter the student's name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob <= Today(clock))
            .WithMessage("Date of birth cannot be in the future.")
            .Must(dob => dob >= Today(clock).AddYears(-MaxAgeYears))
            .WithMessage("Enter a valid date of birth.");

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter a phone number.")
            .Matches(PhonePattern).WithMessage("Enter a valid phone number, for example +91 98765 43210.");
    }

    private static DateOnly Today(TimeProvider clock) =>
        DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
}
