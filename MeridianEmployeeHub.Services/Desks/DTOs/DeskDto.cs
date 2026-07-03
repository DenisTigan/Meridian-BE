namespace MeridianEmployeeHub.Services.Desks.DTOs
{
    public class DeskDto
    {
        public int Id { get; set; }
        public int OfficeId { get; set; }
        public string DeskCode { get; set; } = string.Empty;
        public byte Floor { get; set; }
        public string Zone { get; set; } = string.Empty;
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public bool IsActive { get; set; }
    }
}
