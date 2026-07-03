namespace MeridianEmployeeHub.Services.Offices.DTOs
{
    public class CreateOfficeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int FloorCount { get; set; }
        // TotalDesks NU este acceptat ca input — se calculează automat
    }
}
