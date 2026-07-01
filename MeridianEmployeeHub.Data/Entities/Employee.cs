namespace MeridianEmployeeHub.Data.Entities
{
    // Clasa Employee moștenește BaseEntity, deci va avea automat Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy.
    // Reprezintă tabelul "Employees" din baza de date.
    public class Employee : BaseEntity
    {
        // ── Câmpuri existente ─────────────────────────────────────────────────
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; } // "?" înseamnă că poate fi Null (opțional)
        public string JobTitle { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true; // Folosit pentru "soft delete" (nu ștergem angajatul definitiv)

        // ── Câmpuri noi ───────────────────────────────────────────────────────
        public string? ProfilePictureUrl { get; set; }

        public DateOnly HireDate { get; set; }

        // Forțează schimbarea parolei la primul login
        public bool IsFirstLogin { get; set; } = true;

        public WorkStatus WorkStatus { get; set; } = WorkStatus.Office;

        // ── FK-uri și Navigation properties ─────────────────────────────────
        // FK NOT NULL → Departments
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // FK nullable → Teams
        public int? TeamId { get; set; }
        public Team? Team { get; set; }

        // FK nullable → Employees (self-referencing — managerul direct)
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        // FK NOT NULL → Roles
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        // Subordonații acestui angajat (relația inversă pentru ManagerId)
        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    }
}