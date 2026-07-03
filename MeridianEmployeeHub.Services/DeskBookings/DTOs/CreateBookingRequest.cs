namespace MeridianEmployeeHub.Services.DeskBookings.DTOs
{
    public class CreateBookingRequest
    {
        public int DeskId { get; set; }
        public DateOnly Date { get; set; }
    }
}
