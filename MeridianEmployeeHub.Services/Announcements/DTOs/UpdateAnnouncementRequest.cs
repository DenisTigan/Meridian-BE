using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Announcements.DTOs
{
    // Toate câmpurile sunt nullable — patch-style update (doar câmpurile prezente se aplică)
    public class UpdateAnnouncementRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public AnnouncementCategory? Category { get; set; }
        public bool? IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
