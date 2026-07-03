using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IDeskRepository
    {
        // Returnează desk-urile unui office (toate, inclusiv inactive — Admin vede tot)
        Task<IEnumerable<Desk>> GetByOfficeIdAsync(int officeId);

        // Returnează toate desk-urile active din toate office-urile (pentru disponibilitate globală)
        Task<IEnumerable<Desk>> GetAllActiveAsync();

        Task<Desk?> GetByIdAsync(int id);

        Task AddAsync(Desk desk);
        Task UpdateAsync(Desk desk);
        Task SaveChangesAsync();
    }
}
