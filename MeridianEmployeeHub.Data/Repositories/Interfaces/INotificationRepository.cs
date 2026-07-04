using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(int id);
        Task<IEnumerable<Notification>> GetByEmployeeIdAsync(int employeeId, int skip, int take);
        Task<int> CountUnreadByEmployeeIdAsync(int employeeId);
        
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        
        Task MarkAllAsReadAsync(int employeeId);
        Task SaveChangesAsync();
    }
}
