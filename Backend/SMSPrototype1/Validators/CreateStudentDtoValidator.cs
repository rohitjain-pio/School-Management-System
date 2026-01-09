using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentRqstDto>
    {
        public CreateStudentDtoValidator()
        {
            RuleFor(x => x.SRNumber)
                .NotEmpty().WithMessage("SR Number is required")
                .MaximumLength(20).WithMessage("SR Number cannot exceed 20 characters")
                .Matches("^[a-zA-Z0-9-]+$").WithMessage("SR Number can only contain letters, numbers, and hyphens");

            RuleFor(x => x.RollNumber)
                .NotEmpty().WithMessage("Roll Number is required")
                .GreaterThan(0).WithMessage("Roll Number must be greater than 0")
                .LessThanOrEqualTo(9999).WithMessage("Roll Number cannot exceed 9999");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First Name is required")
                .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("First Name can only contain letters and spaces");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last Name is required")
                .MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Last Name can only contain letters and spaces");

            RuleFor(x => x.DOB)
                .NotEmpty().WithMessage("Date of Birth is required")
                .Must(BeAValidAge).WithMessage("Student must be between 4 and 25 years old");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value");

            RuleFor(x => x.ClassId)
                .NotEmpty().WithMessage("Class ID is required");
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
