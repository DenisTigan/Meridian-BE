using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Calendar.DTOs;
using MeridianEmployeeHub.Services.Calendar.Validators;
using MeridianEmployeeHub.Services.Exceptions;

namespace MeridianEmployeeHub.Services.Calendar
{
    public class CalendarEventService : ICalendarEventService
    {
        private readonly ICalendarEventRepository _repository;

        // Validatoare instanțiate o dată în service — identic cu pattern-ul din EmployeeService
        private readonly CreateCalendarEventRequestValidator _createValidator = new();
        private readonly UpdateCalendarEventRequestValidator _updateValidator = new();

        // Default pentru GET fără filtru de dată: luna curentă (prima zi → ultima zi)
        // Motivare: calendarul se deschide implicit pe luna curentă — potrivit UX.
        // Dacă utilizatorul vrea trecut/viitor, furnizează ?from=&to= explicit.
        private static (DateTime From, DateTime To) CurrentMonthDefault()
        {
            var now = DateTime.UtcNow;
            var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1).AddTicks(-1);
            return (from, to);
        }

        public CalendarEventService(ICalendarEventRepository repository)
        {
            _repository = repository;
        }

        // ── GET toate — filtrate pe interval cu suprapunere + categorie ─────────
        public async Task<IEnumerable<CalendarEventDto>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            EventCategory? category)
        {
            // Default: luna curentă când niciun filtru de dată nu e specificat
            if (!from.HasValue && !to.HasValue)
            {
                var (defaultFrom, defaultTo) = CurrentMonthDefault();
                from = defaultFrom;
                to = defaultTo;
            }

            var events = await _repository.GetFilteredAsync(from, to, category);
            return events.Select(ToDto);
        }

        // ── GET individual ────────────────────────────────────────────────────
        public async Task<CalendarEventDto?> GetByIdAsync(int id)
        {
            var ev = await _repository.GetByIdAsync(id);
            return ev is null ? null : ToDto(ev);
        }

        // ── POST creare ───────────────────────────────────────────────────────
        public async Task<CalendarEventDto> CreateAsync(
            CreateCalendarEventRequest request,
            int createdBy)
        {
            // Validare FluentValidation — aruncă ArgumentException (400) dacă eșuează,
            // identic cu pattern-ul din EmployeeService.UpdateEmployeeAsync
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ArgumentException(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var calendarEvent = new CalendarEvent
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                IsAllDay = request.IsAllDay,
                Category = request.Category,
                Location = request.Location?.Trim(),
                MeetingUrl = request.MeetingUrl?.Trim(),
                CreatedBy = createdBy
            };

            await _repository.AddAsync(calendarEvent);
            await _repository.SaveChangesAsync();

            // Re-fetch cu Include(Creator) pentru CreatedByName în DTO
            var created = await _repository.GetByIdAsync(calendarEvent.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the created event.");

            return ToDto(created);
        }

        // ── PUT actualizare — ownership check: creator sau Admin ─────────────
        // Identic cu AnnouncementService.UpdateAsync — doar câmpul ownership diferă:
        //   Announcement: AuthorId   |   Calendar: CreatedBy
        public async Task<CalendarEventDto> UpdateAsync(
            int id,
            UpdateCalendarEventRequest request,
            int currentUserId,
            bool isAdmin)
        {
            // Validare FluentValidation — identic cu CreateAsync
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ArgumentException(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var calendarEvent = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Calendar event with id {id} not found.");

            // Ownership check — identic cu AnnouncementService, câmp diferit
            if (calendarEvent.CreatedBy != currentUserId && !isAdmin)
                throw new ForbiddenException(
                    "Only the creator or an Admin can edit this event.");

            if (request.Title is not null)
                calendarEvent.Title = request.Title.Trim();

            if (request.Description is not null)
                calendarEvent.Description = request.Description.Trim();

            if (request.Location is not null)
                calendarEvent.Location = request.Location.Trim();

            if (request.MeetingUrl is not null)
                calendarEvent.MeetingUrl = request.MeetingUrl.Trim();

            if (request.IsAllDay.HasValue)
                calendarEvent.IsAllDay = request.IsAllDay.Value;

            if (request.Category.HasValue)
                calendarEvent.Category = request.Category.Value;

            // Aplicare date/ore — validare încrucișată față de valorile efective după patch
            var newStart = request.StartDateTime ?? calendarEvent.StartDateTime;
            var newEnd = request.EndDateTime ?? calendarEvent.EndDateTime;

            if (newEnd <= newStart)
                throw new ArgumentException("EndDateTime must be after StartDateTime.");

            calendarEvent.StartDateTime = newStart;
            calendarEvent.EndDateTime = newEnd;

            await _repository.UpdateAsync(calendarEvent);
            await _repository.SaveChangesAsync();

            return ToDto(calendarEvent);
        }

        // ── DELETE — ownership check: creator sau Admin ───────────────────────
        // Diferit față de AnnouncementService.DeleteAsync (care era strict Admin);
        // conform specificației CalendarEvent, creatorul poate și șterge propriul event.
        public async Task DeleteAsync(int id, int currentUserId, bool isAdmin)
        {
            var calendarEvent = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Calendar event with id {id} not found.");

            if (calendarEvent.CreatedBy != currentUserId && !isAdmin)
                throw new ForbiddenException(
                    "Only the creator or an Admin can delete this event.");

            await _repository.DeleteAsync(calendarEvent);
            await _repository.SaveChangesAsync();
        }

        // ── Mapare manuală CalendarEvent → CalendarEventDto ──────────────────
        // CreatedByName aplanat din navigation property Creator — identic cu
        // AnnouncementMappingProfile.AuthorFullName (FirstName + " " + LastName)
        private static CalendarEventDto ToDto(CalendarEvent e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartDateTime = e.StartDateTime,
            EndDateTime = e.EndDateTime,
            IsAllDay = e.IsAllDay,
            Category = e.Category,
            Location = e.Location,
            MeetingUrl = e.MeetingUrl,
            CreatedBy = e.CreatedBy,
            CreatedByName = e.Creator is not null
                ? e.Creator.FirstName + " " + e.Creator.LastName
                : string.Empty,
            CreatedAt = e.CreatedAt
        };
    }
}
