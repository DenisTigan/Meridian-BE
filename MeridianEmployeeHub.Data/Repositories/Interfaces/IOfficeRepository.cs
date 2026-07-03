using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IOfficeRepository
    {
        // Include(o => o.Desks) — necesar pentru calculul TotalDesks la citire
        Task<IEnumerable<Office>> GetAllAsync();
        Task<Office?> GetByIdAsync(int id);

        Task AddAsync(Office office);
        Task UpdateAsync(Office office);
        Task DeleteAsync(Office office);
        Task SaveChangesAsync();
    }
}
