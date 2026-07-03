using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class DeskRepository : IDeskRepository
    {
        private readonly ApplicationDbContext _context;

        public DeskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET desk-urile unui office — returnează TOATE (inclusiv inactive),
        // astfel Admin vede și desk-urile dezactivate; filtrul vizual e pe client.
        public async Task<IEnumerable<Desk>> GetByOfficeIdAsync(int officeId)
        {
            return await _context.Desks
                .Where(d => d.OfficeId == officeId)
                .OrderBy(d => d.Floor)
                .ThenBy(d => d.DeskCode)
                .ToListAsync();
        }

        // Toate desk-urile active din toate office-urile (pentru disponibilitate globală)
        public async Task<IEnumerable<Desk>> GetAllActiveAsync()
        {
            return await _context.Desks
                .Include(d => d.Office)
                .Where(d => d.IsActive)
                .OrderBy(d => d.OfficeId)
                .ThenBy(d => d.Floor)
                .ThenBy(d => d.DeskCode)
                .ToListAsync();
        }

        public async Task<Desk?> GetByIdAsync(int id)
        {
            return await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Desk desk)
        {
            await _context.Desks.AddAsync(desk);
        }

        public Task UpdateAsync(Desk desk)
        {
            _context.Desks.Update(desk);
            return Task.CompletedTask;
        }

        // DeleteAsync nu există — Desk folosește soft-delete (IsActive = false).
        // DeskBookings din sesiunea 8b va lega bookings de Desk prin FK, deci
        // ștergerea fizică ar rupe istoricul de rezervări.

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
