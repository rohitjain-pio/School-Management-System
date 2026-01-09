using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class CreateAnnouncementDtoValidator : AbstractValidator<CreateAnnouncementRqstDto>
    {
        public CreateAnnouncementDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Detail)
                .NotEmpty().WithMessage("Detail is required")
                .MaximumLength(2000).WithMessage("Detail cannot exceed 2000 characters");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required")
                .Must(BeAValidDate).WithMessage("Date cannot be more than 30 days in the past");

            RuleFor(x => x.AnnouncedBy)
                .NotEmpty().WithMessage("Announced By is required")
                .MaximumLength(100).WithMessage("Announced By cannot exceed 100 characters");

            RuleFor(x => x.SchoolId)
                .NotEmpty().WithMessage("School ID is required");
        }

        private bool BeAValidDate(DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thirtyDaysAgo = today.AddDays(-30);
            return date >= thirtyDaysAgo;
        }
    }
}
