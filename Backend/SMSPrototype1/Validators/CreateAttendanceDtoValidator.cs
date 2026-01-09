using FluentValidation;
using SMSDataModel.Model.RequestDtos;

namespace SMSPrototype1.Validators
{
    public class CreateAttendanceDtoValidator : AbstractValidator<CreateAttendanceRqstDto>
    {
        public CreateAttendanceDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .NotEmpty().WithMessage("Student ID is required");

            RuleFor(x => x.SchoolId)
                .NotEmpty().WithMessage("School ID is required");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required")
                .Must(BeAValidAttendanceDate).WithMessage("Attendance date cannot be in the future or more than 30 days in the past");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(status => new[] { "Present", "Absent", "Late", "Excused" }.Contains(status))
                .WithMessage("Status must be one of: Present, Absent, Late, Excused");
        }

        private bool BeAValidAttendanceDate(DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thirtyDaysAgo = today.AddDays(-30);
            return date >= thirtyDaysAgo && date <= today;
        }
    }
}
