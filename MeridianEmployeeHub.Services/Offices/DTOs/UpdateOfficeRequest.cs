namespace MeridianEmployeeHub.Services.Offices.DTOs
{
    // Toate câmpurile nullable — patch-style: doar câmpurile prezente se aplică
    public class UpdateOfficeRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int? FloorCount { get; set; }
        // TotalDesks NU este acceptat ca input — se calculează automat
    }
}
