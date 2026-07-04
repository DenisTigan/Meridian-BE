namespace MeridianEmployeeHub.Data.Entities
{
    public class WikiArticle
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public WikiCategory? Category { get; set; }

        public int AuthorId { get; set; }
        public Employee? Author { get; set; }

        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
