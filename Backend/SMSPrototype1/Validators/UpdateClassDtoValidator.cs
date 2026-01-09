using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class UpdateClassDtoValidator : AbstractValidator<UpdateClassRequestDto>
    {
        public UpdateClassDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Class Name cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9 ]+$").WithMessage("Class Name can only contain letters, numbers, and spaces")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Section)
                .MaximumLength(5).WithMessage("Section cannot exceed 5 characters")
                .Matches("^[A-Z]+$").WithMessage("Section must be uppercase letters only")
                .When(x => !string.IsNullOrEmpty(x.Section));
        }
    }
}
