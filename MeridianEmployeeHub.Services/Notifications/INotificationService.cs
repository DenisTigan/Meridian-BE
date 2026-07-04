using MeridianEmployeeHub.Services.Notifications.DTOs;

namespace MeridianEmployeeHub.Services.Notifications
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int employeeId, string title, string message, string notificationType, int? relatedEntityId = null, string? relatedEntityType = null);
        
        Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int employeeId, int skip, int take);
        Task<int> GetUnreadCountAsync(int employeeId);
        
        Task<NotificationDto> MarkAsReadAsync(int id, int currentUserId);
        Task MarkAllAsReadAsync(int currentUserId);
    }
}
