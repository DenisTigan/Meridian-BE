using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class WikiCategoryRepository : IWikiCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public WikiCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WikiCategory?> GetByIdAsync(int id)
        {
            return await _context.WikiCategories.FindAsync(id);
        }

        public async Task<WikiCategory?> GetBySlugAsync(string slug)
        {
            return await _context.WikiCategories.FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<WikiCategory>> GetAllAsync()
        {
            return await _context.WikiCategories
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.WikiCategories.AnyAsync(c => c.Slug == slug);
        }

        public async Task AddAsync(WikiCategory category)
        {
            await _context.WikiCategories.AddAsync(category);
        }

        public async Task UpdateAsync(WikiCategory category)
        {
            _context.WikiCategories.Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(WikiCategory category)
        {
            _context.WikiCategories.Remove(category);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
