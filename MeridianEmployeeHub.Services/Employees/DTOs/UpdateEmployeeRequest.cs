namespace MeridianEmployeeHub.Services.Employees.DTOs
{
    // Câmpuri editabile de orice angajat (self-edit): PhoneNumber, ProfilePictureUrl.
    // Câmpurile privilegiate (DepartmentId, RoleId, ManagerId, TeamId, JobTitle, HireDate)
    // pot fi editate DOAR de HR sau Admin — verificare aplicată în EmployeeService.UpdateEmployeeAsync.
    public class UpdateEmployeeRequest
    {
        // ── Câmpuri editabile de orice angajat (self-edit) ────────────────────
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // ── Câmpuri privilegiate (HR/Admin only) ──────────────────────────────
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? JobTitle { get; set; }
        public DateOnly? HireDate { get; set; }
        public int? DepartmentId { get; set; }
        public int? TeamId { get; set; }
        public int? ManagerId { get; set; }
        public int? RoleId { get; set; }
    }
}
