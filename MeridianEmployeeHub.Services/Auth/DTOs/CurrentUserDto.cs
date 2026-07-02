namespace MeridianEmployeeHub.Services.Auth.DTOs
{
    // DTO pentru utilizatorul curent extras din claims-urile JWT.
    // Folosit in controllere/servicii pentru ownership checks si RBAC.
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public bool RequiresPasswordChange { get; set; }

        // Helper pentru ownership checks:
        // if (currentUser.Id != resource.EmployeeId && !currentUser.IsHROrAdmin())
        //     throw new ForbiddenException("Access denied.");
        public bool IsHROrAdmin() =>
            RoleName == "HR" || RoleName == "Admin";

        public bool IsManagerOrAbove() =>
            RoleName == "Manager" || RoleName == "HR" || RoleName == "Admin";

        public bool IsAdmin() =>
            RoleName == "Admin";
    }
}
