using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class CreateSchoolDtoValidator : AbstractValidator<CreateSchoolRequestDto>
    {
        public CreateSchoolDtoValidator()
        {
            RuleFor(x => x.RegistrationNumber)
                .NotEmpty().WithMessage("Registration Number is required")
                .MaximumLength(50).WithMessage("Registration Number cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9/-]+$").WithMessage("Registration Number can only contain letters, numbers, hyphens, and slashes");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("School Name is required")
                .MaximumLength(100).WithMessage("School Name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{10,12}$").WithMessage("Phone number must be 10-12 digits")
                .MaximumLength(12).WithMessage("Phone number cannot exceed 12 characters");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("City can only contain letters and spaces");

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("State is required")
                .MaximumLength(50).WithMessage("State cannot exceed 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("State can only contain letters and spaces");

            RuleFor(x => x.PinCode)
                .NotEmpty().WithMessage("Pincode is required")
                .InclusiveBetween(100000, 999999).WithMessage("Pincode must be a 6-digit number");
        }
    }
}
