using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class WikiArticleRepository : IWikiArticleRepository
    {
        private readonly ApplicationDbContext _context;

        public WikiArticleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WikiArticle?> GetByIdAsync(int id)
        {
            return await _context.WikiArticles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<WikiArticle?> GetBySlugAsync(string slug)
        {
            return await _context.WikiArticles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Slug == slug);
        }

        public async Task<IEnumerable<WikiArticle>> GetArticlesAsync(
            int? categoryId, 
            string? searchTerm, 
            int skip, 
            int take, 
            bool onlyPublished,
            int? authorIdToIncludeUnpublished)
        {
            var query = _context.WikiArticles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .AsQueryable();

            if (onlyPublished)
            {
                if (authorIdToIncludeUnpublished.HasValue)
                {
                    query = query.Where(a => a.IsPublished || a.AuthorId == authorIdToIncludeUnpublished.Value);
                }
                else
                {
                    query = query.Where(a => a.IsPublished);
                }
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.Title.Contains(searchTerm) || a.Content.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.WikiArticles.AnyAsync(a => a.Slug == slug);
        }

        public async Task AddAsync(WikiArticle article)
        {
            await _context.WikiArticles.AddAsync(article);
        }

        public async Task UpdateAsync(WikiArticle article)
        {
            _context.WikiArticles.Update(article);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(WikiArticle article)
        {
            _context.WikiArticles.Remove(article);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
