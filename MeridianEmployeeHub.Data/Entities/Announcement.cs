namespace MeridianEmployeeHub.Data.Entities
{
    // Announcement moștenește BaseEntity → Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
    public class Announcement : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        // Conținut lung — stocat ca TEXT în baza de date (configurat în OnModelCreating)
        public string Content { get; set; } = string.Empty;

        // FK NOT NULL → Employees (autorul anunțului)
        public int AuthorId { get; set; }
        public Employee Author { get; set; } = null!;

        public AnnouncementCategory Category { get; set; } = AnnouncementCategory.General;

        public bool IsPublished { get; set; } = false;

        // Nullable — permite programarea publicării în viitor
        public DateTime? PublishedAt { get; set; }
    }
}
