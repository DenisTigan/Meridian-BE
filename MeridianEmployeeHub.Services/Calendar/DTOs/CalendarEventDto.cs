using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Calendar.DTOs
{
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsAllDay { get; set; }
        public EventCategory Category { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
        public int CreatedBy { get; set; }

        // Identic cu AnnouncementDto.AuthorFullName — aplanat din navigation property Creator
        public string CreatedByName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
