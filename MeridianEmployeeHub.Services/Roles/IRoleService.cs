using MeridianEmployeeHub.Services.Roles.DTOs;

namespace MeridianEmployeeHub.Services.Roles
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto> CreateRoleAsync(string name);
    }
}
