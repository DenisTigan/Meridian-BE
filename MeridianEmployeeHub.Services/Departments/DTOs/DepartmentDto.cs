namespace MeridianEmployeeHub.Services.Departments.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? HeadEmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
