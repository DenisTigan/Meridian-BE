using FluentValidation;
using MeridianEmployeeHub.Services.Calendar.DTOs;

namespace MeridianEmployeeHub.Services.Calendar.Validators
{
    public class UpdateCalendarEventRequestValidator : AbstractValidator<UpdateCalendarEventRequest>
    {
        public UpdateCalendarEventRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(255).WithMessage("Title must be at most 255 characters.")
                .When(x => x.Title is not null);

            RuleFor(x => x.Location)
                .MaximumLength(300).WithMessage("Location must be at most 300 characters.")
                .When(x => x.Location is not null);

            RuleFor(x => x.MeetingUrl)
                .MaximumLength(500).WithMessage("MeetingUrl must be at most 500 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("MeetingUrl must be a valid absolute URL.")
                .When(x => !string.IsNullOrEmpty(x.MeetingUrl));

            // Validare temporală — se aplică NUMAI dacă ambele câmpuri sunt prezente în request.
            // Dacă doar unul e furnizat, validarea completă StartDateTime vs EndDateTime
            // se face în service față de valorile existente ale evenimentului.
            RuleFor(x => x.EndDateTime)
                .GreaterThan(x => x.StartDateTime!.Value)
                .WithMessage("EndDateTime must be after StartDateTime.")
                .When(x => x.StartDateTime.HasValue && x.EndDateTime.HasValue);
        }
    }
}
