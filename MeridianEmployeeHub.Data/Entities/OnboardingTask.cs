namespace MeridianEmployeeHub.Data.Entities
{
    // Reprezintă un task individual dintr-un OnboardingChecklist.
    // Relație many:1 cu OnboardingChecklist.
    // Moștenește BaseEntity (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy).
    public class OnboardingTask : BaseEntity
    {
        // FK NOT NULL → OnboardingChecklists
        public int ChecklistId { get; set; }
        public OnboardingChecklist Checklist { get; set; } = null!;

        // Faza de onboarding căreia îi aparține task-ul
        public OnboardingPhase Phase { get; set; }

        // max 255 — configurat prin Fluent API în OnModelCreating
        public string Title { get; set; } = string.Empty;

        // nullable — descriere opțională a task-ului
        public string? Description { get; set; }

        // Starea de completare a task-ului
        public bool IsCompleted { get; set; } = false;

        // Populat automat când IsCompleted devine true
        public DateTime? CompletedAt { get; set; }

        // nullable, max 100 — identifică task-urile completate automat
        // Exemplu: 'password_changed' → declanșat de AuthService după schimbarea parolei
        public string? AutoTriggerType { get; set; }

        // Ordinea de afișare în UI, per checklist
        public byte OrderIndex { get; set; }
    }
}
