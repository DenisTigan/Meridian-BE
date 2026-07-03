using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Calendar.DTOs;

namespace MeridianEmployeeHub.Services.Calendar
{
    public interface ICalendarEventService
    {
        // GET toate — filtrat pe suprapunere de interval și opțional pe categorie
        Task<IEnumerable<CalendarEventDto>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            EventCategory? category);

        Task<CalendarEventDto?> GetByIdAsync(int id);

        // POST — creatorul e extras din JWT în controller, transmis ca createdBy
        Task<CalendarEventDto> CreateAsync(CreateCalendarEventRequest request, int createdBy);

        // PUT — ownership check în service: creator sau Admin
        Task<CalendarEventDto> UpdateAsync(
            int id,
            UpdateCalendarEventRequest request,
            int currentUserId,
            bool isAdmin);

        // DELETE — ownership check în service: creator sau Admin (diferit față de Announcement unde era strict Admin)
        Task DeleteAsync(int id, int currentUserId, bool isAdmin);
    }
}
