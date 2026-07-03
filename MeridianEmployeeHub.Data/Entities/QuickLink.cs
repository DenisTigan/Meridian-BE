namespace MeridianEmployeeHub.Data.Entities
{
    // QuickLink NU moștenește BaseEntity — este o entitate de configurare simplă,
    // fără audit (CreatedBy/UpdatedBy), similar cu Role din proiect.
    public class QuickLink
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        // Identificator de icon — frontend-ul decide randarea (ex: "slack", "jira", "github")
        public string IconName { get; set; } = string.Empty;

        // Categorie liberă ca string (ex: "Communication", "Development")
        // NU enum — flexibilitate maximă pentru Admin
        public string Category { get; set; } = string.Empty;

        // Ordinea de afișare în cadrul categoriei — byte (0–255 link-uri per categorie)
        public byte OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
