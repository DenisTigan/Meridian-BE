using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.Onboarding.DTOs;

namespace MeridianEmployeeHub.Services.Onboarding
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IOnboardingRepository _onboardingRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public OnboardingService(
            IOnboardingRepository onboardingRepository,
            IEmployeeRepository employeeRepository)
        {
            _onboardingRepository = onboardingRepository;
            _employeeRepository = employeeRepository;
        }

        // ── GET sau CREARE checklist pentru angajatul curent ─────────────────
        public async Task<OnboardingChecklistDto> GetOrCreateChecklistAsync(int employeeId)
        {
            var checklist = await _onboardingRepository.GetByEmployeeIdAsync(employeeId);

            if (checklist is null)
            {
                // Verificăm dacă parola a fost deja schimbată (IsFirstLogin = false)
                // pentru a bifa automat task-ul password_changed la prima creare a checklist-ului.
                var employee = await _employeeRepository.GetByIdAsync(employeeId)
                    ?? throw new KeyNotFoundException($"Employee with id {employeeId} not found.");

                checklist = BuildDefaultChecklist(employeeId, passwordAlreadyChanged: !employee.IsFirstLogin);

                await _onboardingRepository.AddAsync(checklist);
                await _onboardingRepository.SaveChangesAsync();
            }
            else
            {
                // Recalculăm progresul la fiecare citire (date coerente)
                RecalculateProgress(checklist);
            }

            return MapToDto(checklist);
        }

        // ── GET checklist pentru un angajat specificat (ManagerOrAbove) ──────
        public async Task<OnboardingChecklistDto> GetChecklistByEmployeeIdAsync(int employeeId)
        {
            var checklist = await _onboardingRepository.GetByEmployeeIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"No onboarding checklist found for employee {employeeId}.");

            RecalculateProgress(checklist);
            return MapToDto(checklist);
        }

        // ── Completare task individual ────────────────────────────────────────
        public async Task<OnboardingTaskDto> CompleteTaskAsync(int taskId, int currentUserId)
        {
            var task = await _onboardingRepository.GetTaskByIdAsync(taskId)
                ?? throw new KeyNotFoundException($"Onboarding task with id {taskId} not found.");

            // Ownership check: task-ul trebuie să aparțină checklist-ului angajatului curent
            if (task.Checklist.EmployeeId != currentUserId)
                throw new ForbiddenException("You can only complete tasks from your own onboarding checklist.");

            if (!task.IsCompleted)
            {
                task.IsCompleted = true;
                task.CompletedAt = DateTime.UtcNow;

                // Recalculăm progresul pe checklist-ul parinte
                RecalculateProgress(task.Checklist);

                await _onboardingRepository.UpdateAsync(task.Checklist);
                await _onboardingRepository.SaveChangesAsync();
            }

            return MapTaskToDto(task);
        }

        // ── Auto-trigger (apelat din AuthService) ────────────────────────────
        // Dacă checklist-ul nu există → no-op (nu creăm unul).
        // Dacă task-ul există și nu e deja completat → completăm și recalculăm.
        public async Task TriggerAutoCompleteAsync(int employeeId, string triggerType)
        {
            var checklist = await _onboardingRepository.GetByEmployeeIdAsync(employeeId);
            if (checklist is null)
                return; // no-op: checklist-ul va fi creat la prima vizitare a /onboarding/checklist

            var task = checklist.Tasks.FirstOrDefault(t =>
                t.AutoTriggerType == triggerType && !t.IsCompleted);

            if (task is null)
                return; // task-ul nu există sau e deja completat

            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;

            RecalculateProgress(checklist);

            await _onboardingRepository.UpdateAsync(checklist);
            await _onboardingRepository.SaveChangesAsync();
        }

        // ── Metode private helper ─────────────────────────────────────────────

        // Construiește un checklist nou cu task-urile default pentru cele 3 faze.
        // passwordAlreadyChanged: bifează automat task-ul 'password_changed' dacă parola
        // a fost deja schimbată (angajatul a ajuns la onboarding după schimbarea parolei).
        private static OnboardingChecklist BuildDefaultChecklist(int employeeId, bool passwordAlreadyChanged)
        {
            var now = DateTime.UtcNow;

            var tasks = new List<OnboardingTask>
            {
                // ── DayOne ──────────────────────────────────────────────────
                new OnboardingTask
                {
                    Phase = OnboardingPhase.DayOne,
                    Title = "Change your password",
                    Description = "Set a personal password to secure your account.",
                    IsCompleted = passwordAlreadyChanged,
                    CompletedAt = passwordAlreadyChanged ? now : null,
                    AutoTriggerType = "password_changed",
                    OrderIndex = 1
                },
                new OnboardingTask
                {
                    Phase = OnboardingPhase.DayOne,
                    Title = "Complete your profile",
                    Description = "Add your phone number and profile picture.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 2
                },
                new OnboardingTask
                {
                    Phase = OnboardingPhase.DayOne,
                    Title = "Review company policies",
                    Description = "Read and acknowledge the employee handbook and code of conduct.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 3
                },

                // ── WeekOne ─────────────────────────────────────────────────
                new OnboardingTask
                {
                    Phase = OnboardingPhase.WeekOne,
                    Title = "Meet your team",
                    Description = "Attend the team introduction meeting and introduce yourself.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 4
                },
                new OnboardingTask
                {
                    Phase = OnboardingPhase.WeekOne,
                    Title = "Meet your buddy",
                    Description = "Schedule a one-on-one with your onboarding buddy.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 5
                },
                new OnboardingTask
                {
                    Phase = OnboardingPhase.WeekOne,
                    Title = "Set up your workstation",
                    Description = "Install required tools, configure access to systems and repositories.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 6
                },

                // ── FirstMonth ───────────────────────────────────────────────
                new OnboardingTask
                {
                    Phase = OnboardingPhase.FirstMonth,
                    Title = "Complete first project milestone",
                    Description = "Deliver your first assigned task or contribution to the team.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 7
                },
                new OnboardingTask
                {
                    Phase = OnboardingPhase.FirstMonth,
                    Title = "Schedule 1-on-1 with your manager",
                    Description = "Have a 30-minute check-in to discuss expectations and progress.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 8
                },
                new OnboardingTask
                {
                    Phase = OnboardingPhase.FirstMonth,
                    Title = "Review your role objectives",
                    Description = "Align on your OKRs and performance goals for the first quarter.",
                    IsCompleted = false,
                    AutoTriggerType = null,
                    OrderIndex = 9
                }
            };

            var checklist = new OnboardingChecklist
            {
                EmployeeId = employeeId,
                Tasks = tasks
            };

            // Calculăm progresul inițial (relevant dacă password_changed e deja bifat)
            RecalculateProgress(checklist);

            return checklist;
        }

        // OverallProgress = (task-uri completate / total task-uri) * 100, rotunjit la byte (0-100)
        private static void RecalculateProgress(OnboardingChecklist checklist)
        {
            var total = checklist.Tasks.Count;
            if (total == 0)
            {
                checklist.OverallProgress = 0;
                return;
            }

            var completed = checklist.Tasks.Count(t => t.IsCompleted);
            checklist.OverallProgress = (byte)Math.Round((double)completed / total * 100);
        }

        // ── Mappers (fără AutoMapper — entitățile sunt simple și nu necesită profil) ──
        private static OnboardingChecklistDto MapToDto(OnboardingChecklist checklist)
        {
            return new OnboardingChecklistDto
            {
                Id = checklist.Id,
                EmployeeId = checklist.EmployeeId,
                OverallProgress = checklist.OverallProgress,
                CreatedAt = checklist.CreatedAt,
                Tasks = checklist.Tasks
                    .OrderBy(t => t.OrderIndex)
                    .Select(MapTaskToDto)
                    .ToList()
            };
        }

        private static OnboardingTaskDto MapTaskToDto(OnboardingTask task)
        {
            return new OnboardingTaskDto
            {
                Id = task.Id,
                ChecklistId = task.ChecklistId,
                Phase = task.Phase,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CompletedAt = task.CompletedAt,
                AutoTriggerType = task.AutoTriggerType,
                OrderIndex = task.OrderIndex
            };
        }
    }
}
