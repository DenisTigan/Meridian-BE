using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Calendar.DTOs
{
    // Toate câmpurile nullable — patch-style: doar câmpurile prezente se aplică
    public class UpdateCalendarEventRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool? IsAllDay { get; set; }
        public EventCategory? Category { get; set; }
        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }
    }
}
