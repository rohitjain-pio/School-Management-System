using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class UpdateAnnouncementDtoValidator : AbstractValidator<UpdateAnnouncementRequestDto>
    {
        public UpdateAnnouncementDtoValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Detail)
                .MaximumLength(2000).WithMessage("Detail cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Detail));

            RuleFor(x => x.Date)
                .Must(BeAValidDate).WithMessage("Date cannot be more than 30 days in the past")
                .When(x => x.Date != default);

            RuleFor(x => x.AnnouncedBy)
                .MaximumLength(100).WithMessage("Announced By cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.AnnouncedBy));
        }

        private bool BeAValidDate(DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thirtyDaysAgo = today.AddDays(-30);
            return date >= thirtyDaysAgo;
        }
    }
}
