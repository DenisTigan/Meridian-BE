namespace MeridianEmployeeHub.Services.DeskBookings.DTOs
{
    // Răspuns pentru GET /desks/presence — cine e în birou la data cerută
    public class PresenceDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DeskId { get; set; }
        public string DeskCode { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
    }
}
