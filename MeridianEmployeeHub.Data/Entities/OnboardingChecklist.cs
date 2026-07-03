namespace MeridianEmployeeHub.Data.Entities
{
    // Reprezintă checklist-ul de onboarding al unui angajat.
    // Relație 1:1 cu Employee — un angajat are un singur checklist.
    // Moștenește BaseEntity (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy).
    public class OnboardingChecklist : BaseEntity
    {
        // FK NOT NULL → Employees (index unic pentru a forța relația 1:1)
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Progres general calculat dinamic: (task-uri completate / total) * 100
        // Stocat ca byte (0-100) și recalculat la fiecare modificare a unui task.
        public byte OverallProgress { get; set; } = 0;

        // Navigation property — lista completă de task-uri ale acestui checklist
        public ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();
    }
}
