using FluentValidation;
using MeridianEmployeeHub.Services.Calendar.DTOs;

namespace MeridianEmployeeHub.Services.Calendar.Validators
{
    public class CreateCalendarEventRequestValidator : AbstractValidator<CreateCalendarEventRequest>
    {
        public CreateCalendarEventRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title must be at most 255 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(300).WithMessage("Location must be at most 300 characters.")
                .When(x => x.Location is not null);

            RuleFor(x => x.MeetingUrl)
                .MaximumLength(500).WithMessage("MeetingUrl must be at most 500 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("MeetingUrl must be a valid absolute URL.")
                .When(x => !string.IsNullOrEmpty(x.MeetingUrl));

            // Validare temporală: EndDateTime trebuie să fie strict după StartDateTime
            RuleFor(x => x.EndDateTime)
                .GreaterThan(x => x.StartDateTime)
                .WithMessage("EndDateTime must be after StartDateTime.");
        }
    }
}
