using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Announcements.DTOs
{
    public class CreateAnnouncementRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public AnnouncementCategory Category { get; set; } = AnnouncementCategory.General;
        public bool IsPublished { get; set; } = false;

        // Nullable — permite programarea publicării în viitor
        public DateTime? PublishedAt { get; set; }
    }
}
