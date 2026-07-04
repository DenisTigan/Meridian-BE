using System.ComponentModel.DataAnnotations;
using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Wiki.DTOs
{
    public class WikiCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public byte OrderIndex { get; set; }
    }

    public class WikiCategoryTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<WikiCategoryTreeDto> Children { get; set; } = new();
    }

    public class WikiArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCategoryRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        [Range(0, 255)]
        public byte OrderIndex { get; set; }
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
    }

    public class CreateArticleRequest
    {
        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public int CategoryId { get; set; }
        public bool IsPublished { get; set; }
    }

    public class UpdateArticleRequest : CreateArticleRequest
    {
    }
}
