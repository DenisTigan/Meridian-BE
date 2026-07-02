using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();

        // Interogare filtrată + paginată: folosită de GET /api/v1/employees
        Task<(IEnumerable<Employee> Items, int TotalCount)> GetFilteredAsync(
            string? search,
            int? departmentId,
            int? teamId,
            int page,
            int pageSize);

        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByEmailAsync(string email);
        Task<Employee?> GetByRefreshTokenAsync(string refreshToken);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task SaveChangesAsync();
    }
}