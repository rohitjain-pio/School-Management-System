using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentRequestDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.SRNumber)
                .MaximumLength(20).WithMessage("SR Number cannot exceed 20 characters")
                .Matches("^[a-zA-Z0-9-]+$").WithMessage("SR Number can only contain letters, numbers, and hyphens")
                .When(x => !string.IsNullOrEmpty(x.SRNumber));

            RuleFor(x => x.RollNumber)
                .GreaterThan(0).WithMessage("Roll Number must be greater than 0")
                .LessThanOrEqualTo(9999).WithMessage("Roll Number cannot exceed 9999")
                .When(x => x.RollNumber > 0);

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("First Name can only contain letters and spaces")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Last Name can only contain letters and spaces")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.DOB)
                .Must(BeAValidAge).WithMessage("Student must be between 4 and 25 years old")
                .When(x => x.DOB != default);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value")
                .When(x => x.Gender != default);
        }

        private bool BeAValidAge(DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            return age >= 4 && age <= 25;
        }
    }
}
