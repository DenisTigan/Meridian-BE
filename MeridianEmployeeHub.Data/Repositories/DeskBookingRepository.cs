using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class DeskBookingRepository : IDeskBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public DeskBookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Strat 1: verificare coliziune înainte de insert ───────────────────
        public async Task<bool> ExistsConfirmedAsync(int deskId, DateOnly date)
        {
            return await _context.DeskBookings.AnyAsync(b =>
                b.DeskId == deskId &&
                b.BookingDate == date &&
                b.Status == BookingStatus.Confirmed);
        }

        // ── Rezervările unui angajat, paginate ────────────────────────────────
        public async Task<(IEnumerable<DeskBooking> Items, int TotalCount)> GetByEmployeeAsync(
            int employeeId,
            DateOnly? from,
            DateOnly? to,
            int page,
            int pageSize)
        {
            var query = _context.DeskBookings
                .Include(b => b.Desk)
                    .ThenInclude(d => d.Office)
                .Where(b => b.EmployeeId == employeeId)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(b => b.BookingDate >= from.Value);

            if (to.HasValue)
                query = query.Where(b => b.BookingDate <= to.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // ── Toate rezervările dintr-o dată (ManagerOrAbove) ──────────────────
        public async Task<IEnumerable<DeskBooking>> GetByDateAsync(DateOnly date)
        {
            return await _context.DeskBookings
                .Include(b => b.Employee)
                    .ThenInclude(e => e.Department)
                .Include(b => b.Desk)
                    .ThenInclude(d => d.Office)
                .Where(b => b.BookingDate == date)
                .OrderBy(b => b.Employee.LastName)
                .ToListAsync();
        }

        // ── Prezență: angajați cu rezervare Confirmed la data dată ────────────
        public async Task<IEnumerable<DeskBooking>> GetPresenceAsync(DateOnly date, int? departmentId)
        {
            var query = _context.DeskBookings
                .Include(b => b.Employee)
                    .ThenInclude(e => e.Department)
                .Include(b => b.Desk)
                    .ThenInclude(d => d.Office)
                .Where(b =>
                    b.BookingDate == date &&
                    b.Status == BookingStatus.Confirmed)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(b => b.Employee.DepartmentId == departmentId.Value);

            return await query
                .OrderBy(b => b.Employee.LastName)
                .ThenBy(b => b.Employee.FirstName)
                .ToListAsync();
        }

        // ── ID-urile deskurilor deja rezervate Confirmed pentru o dată ────────
        // Folosit de endpoint-ul GET /desks?date= pentru a marca disponibilitatea
        public async Task<HashSet<int>> GetConfirmedDeskIdsForDateAsync(DateOnly date)
        {
            var ids = await _context.DeskBookings
                .Where(b => b.BookingDate == date && b.Status == BookingStatus.Confirmed)
                .Select(b => b.DeskId)
                .ToListAsync();

            return [.. ids];
        }

        public async Task<DeskBooking?> GetByIdAsync(int id)
        {
            return await _context.DeskBookings
                .Include(b => b.Desk)
                    .ThenInclude(d => d.Office)
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(DeskBooking booking)
        {
            await _context.DeskBookings.AddAsync(booking);
        }

        public Task UpdateAsync(DeskBooking booking)
        {
            _context.DeskBookings.Update(booking);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
