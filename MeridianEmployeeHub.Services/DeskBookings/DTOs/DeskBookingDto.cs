using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.DeskBookings.DTOs
{
    public class DeskBookingDto
    {
        public int Id { get; set; }
        public int DeskId { get; set; }
        public string DeskCode { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        // ConfirmedDeskId NU apare în DTO — detaliu intern de persistență
    }
}
