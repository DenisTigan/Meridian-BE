namespace MeridianEmployeeHub.Data.Entities
{
    // Office nu moștenește BaseEntity — entitate de configurare administrativă,
    // similar cu Department (fără UpdatedAt/CreatedBy/UpdatedBy).
    // TotalDesks NU este stocat pe rând — este calculat dinamic la citire
    // ca COUNT(Desks WHERE OfficeId = X AND IsActive = true).
    public class Office
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;        // max 150, configurat Fluent API

        public string Address { get; set; } = string.Empty;     // max 300

        public int FloorCount { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property — necesar pentru calculul TotalDesks la citire
        public ICollection<Desk> Desks { get; set; } = new List<Desk>();
    }
}
