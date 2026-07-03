using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class CalendarEventRepository : ICalendarEventRepository
    {
        private readonly ApplicationDbContext _context;

        public CalendarEventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Filtrare cu suprapunere de intervale — un eveniment apare dacă:
        //   StartDateTime <= to  AND  EndDateTime >= from
        // Acoperă cazul: eveniment care începe înainte de "from" și se termină după "from".
        // Include(Creator) necesar pentru CreatedByName în DTO.
        public async Task<IEnumerable<CalendarEvent>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            EventCategory? category)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Creator)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(e => e.EndDateTime >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.StartDateTime <= to.Value);

            if (category.HasValue)
                query = query.Where(e => e.Category == category.Value);

            return await query
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        // GetByIdAsync include Creator — necesar pentru ownership check și DTO
        public async Task<CalendarEvent?> GetByIdAsync(int id)
        {
            return await _context.CalendarEvents
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(CalendarEvent calendarEvent)
        {
            await _context.CalendarEvents.AddAsync(calendarEvent);
        }

        public Task UpdateAsync(CalendarEvent calendarEvent)
        {
            _context.CalendarEvents.Update(calendarEvent);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(CalendarEvent calendarEvent)
        {
            _context.CalendarEvents.Remove(calendarEvent);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
