namespace MeridianEmployeeHub.Services.DeskBookings.DTOs
{
    // Răspuns pentru GET /desks?date= — lista de deskuri cu disponibilitate per dată
    public class DeskAvailabilityDto
    {
        public int Id { get; set; }
        public int OfficeId { get; set; }
        public string DeskCode { get; set; } = string.Empty;
        public byte Floor { get; set; }
        public string Zone { get; set; } = string.Empty;
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public bool IsActive { get; set; }

        // true dacă NU există rezervare Confirmed pentru data cerută
        public bool IsAvailable { get; set; }
    }
}
