using FluentValidation;
using MeridianEmployeeHub.Services.Employees.DTOs;

namespace MeridianEmployeeHub.Services.Employees.Validators
{
    public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
    {
        public UpdateEmployeeRequestValidator()
        {
            // ── Câmpuri editabile de orice angajat ────────────────────────────
            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must be at most 20 characters.")
                .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Phone number contains invalid characters.")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.ProfilePictureUrl)
                .MaximumLength(2048).WithMessage("Profile picture URL is too long.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("Profile picture URL must be a valid absolute URL.")
                .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));

            // ── Câmpuri privilegiate (HR/Admin) ───────────────────────────────
            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("First name must be at most 50 characters.")
                .When(x => x.FirstName is not null);

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("Last name must be at most 50 characters.")
                .When(x => x.LastName is not null);

            RuleFor(x => x.JobTitle)
                .MaximumLength(100).WithMessage("Job title must be at most 100 characters.")
                .When(x => x.JobTitle is not null);

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("DepartmentId must be a positive integer.")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("RoleId must be a positive integer.")
                .When(x => x.RoleId.HasValue);
        }
    }
}
