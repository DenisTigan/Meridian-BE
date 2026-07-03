using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IDeskRepository
    {
        // Returnează desk-urile unui office (toate, inclusiv inactive — Admin vede tot)
        Task<IEnumerable<Desk>> GetByOfficeIdAsync(int officeId);
        Task<Desk?> GetByIdAsync(int id);

        Task AddAsync(Desk desk);
        Task UpdateAsync(Desk desk);
        Task SaveChangesAsync();
    }
}
