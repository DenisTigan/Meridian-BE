using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.HRTickets.DTOs
{
    public class UpdateTicketStatusRequest
    {
        public TicketStatus Status { get; set; }
    }
}
