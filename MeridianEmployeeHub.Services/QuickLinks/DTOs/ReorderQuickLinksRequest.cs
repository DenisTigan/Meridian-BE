namespace MeridianEmployeeHub.Services.QuickLinks.DTOs
{
    public class ReorderQuickLinksRequest
    {
        // ID-uri în ordinea dorită — primul element primește OrderIndex = 0, etc.
        public List<int> OrderedIds { get; set; } = [];
    }
}
