using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.Wiki.DTOs;

namespace MeridianEmployeeHub.Services.Wiki
{
    public class WikiArticleService : IWikiArticleService
    {
        private readonly IWikiArticleRepository _repository;
        private readonly IWikiCategoryRepository _categoryRepository;

        public WikiArticleService(
            IWikiArticleRepository repository,
            IWikiCategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<WikiArticleDto>> GetArticlesAsync(
            int? categoryId, 
            string? searchTerm, 
            int skip, 
            int take, 
            bool isHROrAdmin, 
            int currentUserId)
        {
            bool onlyPublished = !isHROrAdmin;
            int? authorIdToIncludeUnpublished = isHROrAdmin ? null : currentUserId;

            var articles = await _repository.GetArticlesAsync(
                categoryId, 
                searchTerm, 
                skip, 
                take, 
                onlyPublished, 
                authorIdToIncludeUnpublished);

            return articles.Select(MapToDto);
        }

        public async Task<WikiArticleDto> GetArticleBySlugAsync(string slug, bool isHROrAdmin, int currentUserId)
        {
            var article = await _repository.GetBySlugAsync(slug)
                ?? throw new KeyNotFoundException($"Article {slug} not found.");

            // Visibility check
            if (!article.IsPublished && article.AuthorId != currentUserId && !isHROrAdmin)
            {
                throw new ForbiddenException("You do not have permission to view this unpublished article.");
            }

            // Increment ViewCount synchronously
            article.ViewCount++;
            await _repository.UpdateAsync(article);
            await _repository.SaveChangesAsync();

            return MapToDto(article);
        }

        public async Task<WikiArticleDto> CreateArticleAsync(CreateArticleRequest request, int currentUserId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId)
                ?? throw new KeyNotFoundException("Category not found.");

            var baseSlug = SlugGenerator.GenerateSlug(request.Title);
            var finalSlug = await GenerateUniqueSlugAsync(baseSlug);

            var article = new WikiArticle
            {
                Title = request.Title,
                Slug = finalSlug,
                Content = request.Content,
                CategoryId = request.CategoryId,
                AuthorId = currentUserId,
                IsPublished = request.IsPublished,
                ViewCount = 0
            };

            await _repository.AddAsync(article);
            await _repository.SaveChangesAsync();

            return MapToDto(article);
        }

        public async Task<WikiArticleDto> UpdateArticleAsync(string slug, UpdateArticleRequest request, bool isHROrAdmin, int currentUserId)
        {
            var article = await _repository.GetBySlugAsync(slug)
                ?? throw new KeyNotFoundException($"Article {slug} not found.");

            // Ownership check
            if (article.AuthorId != currentUserId && !isHROrAdmin)
            {
                throw new ForbiddenException("Only the author or HR/Admin can edit this article.");
            }

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId)
                ?? throw new KeyNotFoundException("Category not found.");

            if (article.Title != request.Title)
            {
                article.Title = request.Title;
                var baseSlug = SlugGenerator.GenerateSlug(request.Title);
                article.Slug = await GenerateUniqueSlugAsync(baseSlug, article.Id);
            }

            article.Content = request.Content;
            article.CategoryId = request.CategoryId;
            article.IsPublished = request.IsPublished;

            await _repository.UpdateAsync(article);
            await _repository.SaveChangesAsync();

            return MapToDto(article);
        }

        public async Task DeleteArticleAsync(string slug)
        {
            var article = await _repository.GetBySlugAsync(slug)
                ?? throw new KeyNotFoundException($"Article {slug} not found.");

            await _repository.DeleteAsync(article);
            await _repository.SaveChangesAsync();
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, int? excludeId = null)
        {
            var slug = baseSlug;
            int counter = 2;

            while (true)
            {
                var existing = await _repository.GetBySlugAsync(slug);
                if (existing == null || existing.Id == excludeId)
                {
                    return slug;
                }
                
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
        }

        private WikiArticleDto MapToDto(WikiArticle article)
        {
            return new WikiArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Content = article.Content,
                CategoryId = article.CategoryId,
                CategoryName = article.Category?.Name,
                AuthorId = article.AuthorId,
                AuthorName = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}" : null,
                IsPublished = article.IsPublished,
                ViewCount = article.ViewCount,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt
            };
        }
    }
}
