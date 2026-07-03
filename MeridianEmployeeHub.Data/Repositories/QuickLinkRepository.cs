using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class QuickLinkRepository : IQuickLinkRepository
    {
        private readonly ApplicationDbContext _context;

        public QuickLinkRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Returnează link-urile active sortate: Category → OrderIndex
        public async Task<IEnumerable<QuickLink>> GetAllActiveAsync()
        {
            return await _context.QuickLinks
                .Where(q => q.IsActive)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.OrderIndex)
                .ToListAsync();
        }

        public async Task<QuickLink?> GetByIdAsync(int id)
        {
            return await _context.QuickLinks.FindAsync(id);
        }

        // Încarcă toate entitățile cu ID-urile date într-un singur query (pentru reorder batch)
        public async Task<IEnumerable<QuickLink>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.QuickLinks
                .Where(q => ids.Contains(q.Id))
                .ToListAsync();
        }

        public async Task AddAsync(QuickLink quickLink)
        {
            await _context.QuickLinks.AddAsync(quickLink);
        }

        public Task UpdateAsync(QuickLink quickLink)
        {
            _context.QuickLinks.Update(quickLink);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(QuickLink quickLink)
        {
            _context.QuickLinks.Remove(quickLink);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
