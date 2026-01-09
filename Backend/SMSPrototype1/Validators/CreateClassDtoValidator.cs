using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class CreateClassDtoValidator : AbstractValidator<CreateClassRequestDto>
    {
        public CreateClassDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Class Name is required")
                .MaximumLength(50).WithMessage("Class Name cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9 ]+$").WithMessage("Class Name can only contain letters, numbers, and spaces");

            RuleFor(x => x.Section)
                .NotEmpty().WithMessage("Section is required")
                .MaximumLength(5).WithMessage("Section cannot exceed 5 characters")
                .Matches("^[A-Z]+$").WithMessage("Section must be uppercase letters only");

            RuleFor(x => x.SchoolId)
                .NotEmpty().WithMessage("School ID is required");
        }
    }
}
