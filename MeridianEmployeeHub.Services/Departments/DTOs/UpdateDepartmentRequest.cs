namespace MeridianEmployeeHub.Services.Departments.DTOs
{
    // DTO pentru operatia PUT /api/v1/departments/{id}
    // Toate câmpurile sunt opționale (null = nu se modifică).
    public class UpdateDepartmentRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? HeadEmployeeId { get; set; }
    }
}
