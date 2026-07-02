using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Employees.DTOs
{
    // DTO pentru PATCH /api/v1/employees/{id}/work-status
    public class UpdateWorkStatusRequest
    {
        public WorkStatus WorkStatus { get; set; }
    }
}
