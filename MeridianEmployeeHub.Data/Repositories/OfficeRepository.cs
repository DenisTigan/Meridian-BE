using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class OfficeRepository : IOfficeRepository
    {
        private readonly ApplicationDbContext _context;

        public OfficeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Include(Desks) necesar pentru calculul TotalDesks în service/mapper
        public async Task<IEnumerable<Office>> GetAllAsync()
        {
            return await _context.Offices
                .Include(o => o.Desks)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<Office?> GetByIdAsync(int id)
        {
            return await _context.Offices
                .Include(o => o.Desks)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(Office office)
        {
            await _context.Offices.AddAsync(office);
        }

        public Task UpdateAsync(Office office)
        {
            _context.Offices.Update(office);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Office office)
        {
            _context.Offices.Remove(office);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
