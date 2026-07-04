namespace MeridianEmployeeHub.Data.Entities
{
    public class WikiCategory
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        
        public int? ParentCategoryId { get; set; }
        public WikiCategory? ParentCategory { get; set; }

        public byte OrderIndex { get; set; }

        public ICollection<WikiCategory> SubCategories { get; set; } = new List<WikiCategory>();
        public ICollection<WikiArticle> Articles { get; set; } = new List<WikiArticle>();
    }
}
