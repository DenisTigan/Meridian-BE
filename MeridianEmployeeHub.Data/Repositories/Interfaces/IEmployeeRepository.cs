using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByEmailAsync(string email);
        Task<Employee?> GetByRefreshTokenAsync(string refreshToken);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task SaveChangesAsync();
    }
}