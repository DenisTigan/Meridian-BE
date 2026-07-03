using MeridianEmployeeHub.Services.Onboarding.DTOs;

namespace MeridianEmployeeHub.Services.Onboarding
{
    public interface IOnboardingService
    {
        // Returnează checklist-ul angajatului curent.
        // Dacă nu există, îl creează cu task-urile default și îl returnează.
        // La creare, bifează automat task-ul 'password_changed' dacă parola a fost deja schimbată.
        Task<OnboardingChecklistDto> GetOrCreateChecklistAsync(int employeeId);

        // Returnează checklist-ul unui angajat specificat.
        // Aruncă KeyNotFoundException dacă angajatul nu are checklist.
        // Folosit de ManagerOrAbove pentru GET /checklist/{employeeId}.
        Task<OnboardingChecklistDto> GetChecklistByEmployeeIdAsync(int employeeId);

        // Marchează un task ca și completat.
        // Verifică ownership: task-ul trebuie să aparțină checklist-ului angajatului curent.
        // Aruncă ForbiddenException dacă nu e self-owned, KeyNotFoundException dacă task-ul nu există.
        // Recalculează OverallProgress după marcare.
        Task<OnboardingTaskDto> CompleteTaskAsync(int taskId, int currentUserId);

        // Auto-trigger: caută un task cu AutoTriggerType == triggerType în checklist-ul angajatului.
        // Dacă angajatul nu are checklist → no-op (nu creează).
        // Dacă task-ul există și nu e deja completat → îl marchează și recalculează progresul.
        // Apelat din AuthService.ChangePasswordAsync cu triggerType = "password_changed".
        Task TriggerAutoCompleteAsync(int employeeId, string triggerType);
    }
}
