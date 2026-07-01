namespace MeridianEmployeeHub.Services.Departments.DTOs
{
    public class CreateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? HeadEmployeeId { get; set; }
    }
}
