using MeridianEmployeeHub.Services.Wiki.DTOs;

namespace MeridianEmployeeHub.Services.Wiki
{
    public interface IWikiCategoryService
    {
        Task<IEnumerable<WikiCategoryTreeDto>> GetCategoryTreeAsync();
        
        Task<WikiCategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
        Task<WikiCategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(int id);
    }
}
