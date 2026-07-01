using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Employees.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string JobTitle { get; set; } = string.Empty;

        // Câmpuri noi
        public string? ProfilePictureUrl { get; set; }
        public DateOnly HireDate { get; set; }
        public bool IsFirstLogin { get; set; }
        public WorkStatus WorkStatus { get; set; }
        public int DepartmentId { get; set; }
        public int? TeamId { get; set; }
        public int? ManagerId { get; set; }
        public int RoleId { get; set; }
    }
}