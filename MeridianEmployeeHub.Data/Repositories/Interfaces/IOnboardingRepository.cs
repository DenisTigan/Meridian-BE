using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IOnboardingRepository
    {
        // Caută checklist-ul angajatului incluzând toate task-urile asociate.
        // Returnează null dacă angajatul nu are încă un checklist.
        Task<OnboardingChecklist?> GetByEmployeeIdAsync(int employeeId);

        // Caută un task individual după Id (fără Include — folosit pentru ownership check).
        Task<OnboardingTask?> GetTaskByIdAsync(int taskId);

        // Adaugă un checklist nou (cu task-urile din collection).
        Task AddAsync(OnboardingChecklist checklist);

        // Marchează checklist-ul ca modificat (EF tracked).
        Task UpdateAsync(OnboardingChecklist checklist);

        Task SaveChangesAsync();
    }
}
