using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IWikiCategoryRepository
    {
        Task<WikiCategory?> GetByIdAsync(int id);
        Task<WikiCategory?> GetBySlugAsync(string slug);
        Task<IEnumerable<WikiCategory>> GetAllAsync();
        
        Task<bool> SlugExistsAsync(string slug);
        
        Task AddAsync(WikiCategory category);
        Task UpdateAsync(WikiCategory category);
        Task DeleteAsync(WikiCategory category);
        
        Task SaveChangesAsync();
    }
}
