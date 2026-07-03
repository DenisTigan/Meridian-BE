namespace MeridianEmployeeHub.Services.Offices.DTOs
{
    public class OfficeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int FloorCount { get; set; }

        // Calculat dinamic la citire: COUNT(Desks WHERE IsActive = true)
        // NU este stocat în baza de date — întotdeauna reflectă starea reală
        public int TotalDesks { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
