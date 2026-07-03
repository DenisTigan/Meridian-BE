using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IAnnouncementRepository
    {
        // Returnează lista filtrată și paginată; isPrivileged determină vizibilitatea nepubblicate
        Task<(IEnumerable<Announcement> Items, int TotalCount)> GetFilteredAsync(
            AnnouncementCategory? category,
            bool isPrivileged,
            int currentUserId,
            int page,
            int pageSize);

        Task<Announcement?> GetByIdAsync(int id);

        Task AddAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(Announcement announcement);
        Task SaveChangesAsync();
    }
}
