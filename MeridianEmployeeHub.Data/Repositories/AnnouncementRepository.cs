using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Interogare filtrată + paginată ────────────────────────────────────
        // Vizibilitate:
        //   • Utilizatori privilegiați (HR/Admin) văd TOATE anunțurile.
        //   • Autorul vede propriile anunțuri nepublicate (preview).
        //   • Restul angajaților văd DOAR anunțurile cu IsPublished = true.
        public async Task<(IEnumerable<Announcement> Items, int TotalCount)> GetFilteredAsync(
            AnnouncementCategory? category,
            bool isPrivileged,
            int currentUserId,
            int page,
            int pageSize)
        {
            var query = _context.Announcements
                .Include(a => a.Author)
                .AsQueryable();

            // Filtru vizibilitate
            if (!isPrivileged)
            {
                // Angajat obișnuit: vede publicatele + propriile anunțuri nepublicate
                query = query.Where(a => a.IsPublished || a.AuthorId == currentUserId);
            }

            // Filtru opțional pe categorie
            if (category.HasValue)
                query = query.Where(a => a.Category == category.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            return await _context.Announcements
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Announcement announcement)
        {
            await _context.Announcements.AddAsync(announcement);
        }

        public Task UpdateAsync(Announcement announcement)
        {
            _context.Announcements.Update(announcement);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Announcement announcement)
        {
            _context.Announcements.Remove(announcement);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
