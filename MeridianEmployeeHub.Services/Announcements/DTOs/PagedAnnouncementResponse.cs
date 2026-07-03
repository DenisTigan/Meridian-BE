namespace MeridianEmployeeHub.Services.Announcements.DTOs
{
    public class PagedAnnouncementResponse
    {
        public IEnumerable<AnnouncementDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
