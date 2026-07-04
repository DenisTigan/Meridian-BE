using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IWikiArticleRepository
    {
        Task<WikiArticle?> GetByIdAsync(int id);
        Task<WikiArticle?> GetBySlugAsync(string slug);
        
        Task<IEnumerable<WikiArticle>> GetArticlesAsync(
            int? categoryId, 
            string? searchTerm, 
            int skip, 
            int take, 
            bool onlyPublished,
            int? authorIdToIncludeUnpublished);
            
        Task<bool> SlugExistsAsync(string slug);
        
        Task AddAsync(WikiArticle article);
        Task UpdateAsync(WikiArticle article);
        Task DeleteAsync(WikiArticle article);
        
        Task SaveChangesAsync();
    }
}
