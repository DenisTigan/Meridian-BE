namespace MeridianEmployeeHub.Services.Desks.DTOs
{
    // Toate câmpurile nullable — patch-style: doar câmpurile prezente se aplică
    public class UpdateDeskRequest
    {
        public string? DeskCode { get; set; }
        public byte? Floor { get; set; }
        public string? Zone { get; set; }
        public decimal? PositionX { get; set; }
        public decimal? PositionY { get; set; }
        public bool? IsActive { get; set; }
        // OfficeId nu este editabil după creare — desk-ul rămâne legat de office-ul original
    }
}
