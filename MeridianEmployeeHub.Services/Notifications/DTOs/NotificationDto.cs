namespace MeridianEmployeeHub.Services.Notifications.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        
        public bool IsRead { get; set; }

        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
