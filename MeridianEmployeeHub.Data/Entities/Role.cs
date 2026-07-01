namespace MeridianEmployeeHub.Data.Entities
{
    // Entitate simplă de lookup pentru roluri — nu moștenește BaseEntity
    // deoarece rolurile sunt date de referință statice (Employee, Manager, HR, Admin).
    public class Role
    {
        public int Id { get; set; }

        // Valorile așteptate: "Employee", "Manager", "HR", "Admin"
        public string Name { get; set; } = string.Empty;

        // Navigation property — angajații cu acest rol
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
