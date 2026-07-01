using MeridianEmployeeHub.Services.Teams.DTOs;

namespace MeridianEmployeeHub.Services.Teams
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();
        Task<TeamDto?> GetTeamByIdAsync(int id);
        Task<TeamDto> CreateTeamAsync(CreateTeamRequest request);
    }
}
