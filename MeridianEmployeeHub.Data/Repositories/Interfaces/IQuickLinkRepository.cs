using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IQuickLinkRepository
    {
        // Returnează toate link-urile active, sortate Category → OrderIndex
        Task<IEnumerable<QuickLink>> GetAllActiveAsync();

        Task<QuickLink?> GetByIdAsync(int id);

        // Returnează toate link-urile cu ID-urile specificate (folosit la reorder batch)
        Task<IEnumerable<QuickLink>> GetByIdsAsync(IEnumerable<int> ids);

        Task AddAsync(QuickLink quickLink);
        Task UpdateAsync(QuickLink quickLink);
        Task DeleteAsync(QuickLink quickLink);
        Task SaveChangesAsync();
    }
}
