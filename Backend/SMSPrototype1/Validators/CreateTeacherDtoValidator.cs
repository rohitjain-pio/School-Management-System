using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class CreateTeacherDtoValidator : AbstractValidator<CreateTeacherRqstDto>
    {
        public CreateTeacherDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Name can only contain letters and spaces");

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

            RuleFor(x => x.gender)
                .IsInEnum().WithMessage("Invalid gender value");

            RuleFor(x => x.SchoolId)
                .NotEmpty().WithMessage("School ID is required");
        }
    }
}
