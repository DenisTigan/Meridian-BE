using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface ICalendarEventRepository
    {
        // Filtrare cu suprapunere de intervale: StartDateTime <= to AND EndDateTime >= from
        Task<IEnumerable<CalendarEvent>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            EventCategory? category);

        Task<CalendarEvent?> GetByIdAsync(int id);

        Task AddAsync(CalendarEvent calendarEvent);
        Task UpdateAsync(CalendarEvent calendarEvent);
        Task DeleteAsync(CalendarEvent calendarEvent);
        Task SaveChangesAsync();
    }
}
