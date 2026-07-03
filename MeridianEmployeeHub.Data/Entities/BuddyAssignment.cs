namespace MeridianEmployeeHub.Data.Entities
{
    // Reprezintă o asignare de tip Buddy între doi angajați.
    // NewEmployee — angajatul nou care are nevoie de ghidare
    // Buddy       — angajatul experimentat care ghidează
    //
    // Regulă de business: un angajat poate fi NewEmployeeId în CEL MULT un assignment
    // cu Status = Active la un moment dat. Poate fi BuddyId în oricâte simultan.
    //
    // NU moștenește BaseEntity: are AssignedAt explicit (momentul semantic al asignării),
    // fără UpdatedAt/CreatedBy/UpdatedBy — pattern similar cu Department.
    public class BuddyAssignment
    {
        public int Id { get; set; }

        // FK NOT NULL → Employees (angajatul nou)
        // Configurat explicit cu Restrict în Fluent API pentru a evita cascade paths
        public int NewEmployeeId { get; set; }
        public Employee NewEmployee { get; set; } = null!;

        // FK NOT NULL → Employees (buddy-ul asignat)
        // Configurat explicit cu Restrict în Fluent API pentru a evita cascade paths
        public int BuddyId { get; set; }
        public Employee Buddy { get; set; } = null!;

        // Data și ora la care a fost creată asignarea
        public DateTime AssignedAt { get; set; }

        // Note opționale (ex. arii de focalizare, context specific)
        public string? Notes { get; set; }

        // Starea curentă a asignării
        public BuddyStatus Status { get; set; } = BuddyStatus.Active;
    }
}
