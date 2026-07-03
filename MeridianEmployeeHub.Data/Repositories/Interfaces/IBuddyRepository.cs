using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IBuddyRepository
    {
        // Returnează toate assignment-urile (include NewEmployee și Buddy pentru DTO-uri cu FullName)
        Task<IEnumerable<BuddyAssignment>> GetAllAsync();

        // Caută un assignment după Id (include navigation properties)
        Task<BuddyAssignment?> GetByIdAsync(int id);

        // Caută assignment-ul ACTIV al unui angajat ca new hire
        // Returnează null dacă nu există — folosit de GET /my-assignment și de regula de business
        Task<BuddyAssignment?> GetActiveByNewEmployeeIdAsync(int newEmployeeId);

        Task AddAsync(BuddyAssignment assignment);
        Task UpdateAsync(BuddyAssignment assignment);
        Task SaveChangesAsync();
    }
}
