namespace MeridianEmployeeHub.Data.Entities
{
    // Team aparține unui Department și poate avea un TeamLead (angajat).
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;       // max 100, configurat prin Fluent API
        public string? Description { get; set; }               // nullable

        // FK NOT NULL → Departments
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // FK nullable → Employees (team lead-ul echipei)
        public int? TeamLeadId { get; set; }
        public Employee? TeamLead { get; set; }

        // Navigation properties
        public ICollection<Employee> Members { get; set; } = new List<Employee>();
    }
}
