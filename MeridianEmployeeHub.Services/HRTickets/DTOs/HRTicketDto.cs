using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.HRTickets.DTOs
{
    public class HRTicketDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;

        // Cine a depus tichetul
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public TicketCategory Category { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }

        // Angajatul HR/Admin asignat — null dacă neasisgnat
        public int? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }

        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
