namespace MeridianEmployeeHub.Services.Employees.DTOs
{
    public class BadgeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string QrCodeData { get; set; } = string.Empty;
    }
}
