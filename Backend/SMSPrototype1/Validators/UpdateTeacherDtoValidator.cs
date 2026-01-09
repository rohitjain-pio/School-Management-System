using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class UpdateTeacherDtoValidator : AbstractValidator<UpdateTeacherRequestDto>
    {
        public UpdateTeacherDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Name can only contain letters and spaces")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .Matches(@"^\d{10,12}$").WithMessage("Phone number must be 10-12 digits")
                .MaximumLength(12).WithMessage("Phone number cannot exceed 12 characters")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value")
                .When(x => x.Gender != default);
        }
    }
}
