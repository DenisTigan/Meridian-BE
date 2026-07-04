using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.HRTickets.DTOs
{
    public class CreateTicketRequest
    {
        public TicketCategory Category { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
