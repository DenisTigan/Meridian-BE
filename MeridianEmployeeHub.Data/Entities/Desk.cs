namespace MeridianEmployeeHub.Data.Entities
{
    // Desk nu moștenește BaseEntity — entitate de configurare gestionată de Admin.
    // Soft-delete prin IsActive = false (NU ștergere fizică), pentru că sesiunea 8b
    // va lega DeskBookings de Desk prin FK — ștergerea fizică ar rupe istoricul.
    public class Desk
    {
        public int Id { get; set; }

        // FK NOT NULL → Offices
        public int OfficeId { get; set; }
        public Office Office { get; set; } = null!;

        public string DeskCode { get; set; } = string.Empty;   // ex. "A-14", max 20

        public byte Floor { get; set; }                         // etajul pe care se află biroul

        public string Zone { get; set; } = string.Empty;        // ex. "Engineering Row", max 50

        // Coordonate pentru harta interactivă — Milestone 5, stocate acum, nefolosite
        public decimal PositionX { get; set; }                  // precision 6, scale 2
        public decimal PositionY { get; set; }                  // precision 6, scale 2

        public bool IsActive { get; set; } = true;
    }
}
