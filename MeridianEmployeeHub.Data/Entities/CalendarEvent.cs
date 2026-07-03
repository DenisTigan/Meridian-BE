namespace MeridianEmployeeHub.Data.Entities
{
    // CalendarEvent nu moștenește BaseEntity deoarece nu avem nevoie de UpdatedAt/UpdatedBy
    // automată — managementul creatorului se face prin câmpul explicit CreatedBy (FK).
    public class CalendarEvent
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;          // max 255, configurat Fluent API

        public string? Description { get; set; }                    // nullable

        public DateTime StartDateTime { get; set; }                 // UTC

        public DateTime EndDateTime { get; set; }                   // UTC; > StartDateTime (validat în service)

        public bool IsAllDay { get; set; }

        public EventCategory Category { get; set; }

        public string? Location { get; set; }                       // nullable, max 300

        public string? MeetingUrl { get; set; }                     // nullable, max 500

        // FK NOT NULL → Employees (creatorul evenimentului — ownership pentru update/delete)
        public int CreatedBy { get; set; }
        public Employee Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
