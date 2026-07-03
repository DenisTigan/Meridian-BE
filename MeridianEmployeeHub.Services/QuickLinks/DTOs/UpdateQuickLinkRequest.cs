namespace MeridianEmployeeHub.Services.QuickLinks.DTOs
{
    // Toate câmpurile nullable — patch-style: doar câmpurile prezente se aplică
    public class UpdateQuickLinkRequest
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? IconName { get; set; }
        public string? Category { get; set; }
        public byte? OrderIndex { get; set; }
        public bool? IsActive { get; set; }
    }
}
