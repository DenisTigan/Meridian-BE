using MeridianEmployeeHub.Services.Departments.DTOs;

namespace MeridianEmployeeHub.Services.Departments
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request);
        Task<DepartmentDto> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request);
        Task DeleteDepartmentAsync(int id);
    }
}
