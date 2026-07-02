using MeridianEmployeeHub.Services.Teams.DTOs;

namespace MeridianEmployeeHub.Services.Teams
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();
        Task<IEnumerable<TeamDto>> GetByDepartmentAsync(int departmentId);
        Task<TeamDto?> GetTeamByIdAsync(int id);
        Task<TeamDto> CreateTeamAsync(CreateTeamRequest request);
        Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamRequest request);
        Task DeleteTeamAsync(int id);
    }
}
