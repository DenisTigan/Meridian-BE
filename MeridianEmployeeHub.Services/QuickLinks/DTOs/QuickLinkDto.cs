namespace MeridianEmployeeHub.Services.QuickLinks.DTOs
{
    public class QuickLinkDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public byte OrderIndex { get; set; }
        public bool IsActive { get; set; }
    }
}
