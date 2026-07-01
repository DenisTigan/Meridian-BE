namespace MeridianEmployeeHub.Data.Entities
{
    // Department nu moștenește BaseEntity deoarece are doar CreatedAt (fără UpdatedAt, CreatedBy etc.)
    // Conform specificației: Id, Name, Description, HeadEmployeeId, CreatedAt.
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;       // max 100, configurat prin Fluent API
        public string? Description { get; set; }               // nullable

        // FK nullable către Employees (angajatul care conduce departamentul)
        public int? HeadEmployeeId { get; set; }
        public Employee? HeadEmployee { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}
