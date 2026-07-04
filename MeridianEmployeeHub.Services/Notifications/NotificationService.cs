using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.Notifications.DTOs;

namespace MeridianEmployeeHub.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateNotificationAsync(int employeeId, string title, string message, string notificationType, int? relatedEntityId = null, string? relatedEntityType = null)
        {
            var notification = new Notification
            {
                EmployeeId = employeeId,
                Title = title,
                Message = message,
                NotificationType = notificationType,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                IsRead = false
            };

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int employeeId, int skip, int take)
        {
            var notifications = await _repository.GetByEmployeeIdAsync(employeeId, skip, take);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                IsRead = n.IsRead,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            return await _repository.CountUnreadByEmployeeIdAsync(employeeId);
        }

        public async Task<NotificationDto> MarkAsReadAsync(int id, int currentUserId)
        {
            var notification = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Notification with id {id} not found.");

            if (notification.EmployeeId != currentUserId)
            {
                throw new ForbiddenException("You cannot mark a notification that doesn't belong to you as read.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _repository.UpdateAsync(notification);
                await _repository.SaveChangesAsync();
            }

            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                IsRead = notification.IsRead,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task MarkAllAsReadAsync(int currentUserId)
        {
            await _repository.MarkAllAsReadAsync(currentUserId);
            // No SaveChangesAsync needed because MarkAllAsReadAsync uses ExecuteUpdateAsync which is immediate
        }
    }
}
