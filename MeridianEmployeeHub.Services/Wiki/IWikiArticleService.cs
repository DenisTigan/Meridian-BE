using MeridianEmployeeHub.Services.Wiki.DTOs;

namespace MeridianEmployeeHub.Services.Wiki
{
    public interface IWikiArticleService
    {
        Task<IEnumerable<WikiArticleDto>> GetArticlesAsync(
            int? categoryId, 
            string? searchTerm, 
            int skip, 
            int take, 
            bool isHROrAdmin, 
            int currentUserId);
            
        Task<WikiArticleDto> GetArticleBySlugAsync(string slug, bool isHROrAdmin, int currentUserId);
        
        Task<WikiArticleDto> CreateArticleAsync(CreateArticleRequest request, int currentUserId);
        Task<WikiArticleDto> UpdateArticleAsync(string slug, UpdateArticleRequest request, bool isHROrAdmin, int currentUserId);
        Task DeleteArticleAsync(string slug);
    }
}
