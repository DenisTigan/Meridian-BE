using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Teams.DTOs;

namespace MeridianEmployeeHub.Services.Teams
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _repository;
        private readonly IMapper _mapper;

        public TeamService(ITeamRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            var teams = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }

        public async Task<TeamDto?> GetTeamByIdAsync(int id)
        {
            var team = await _repository.GetByIdAsync(id);
            return team == null ? null : _mapper.Map<TeamDto>(team);
        }

        public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request)
        {
            var team = _mapper.Map<Team>(request);
            await _repository.AddAsync(team);
            await _repository.SaveChangesAsync();
            return _mapper.Map<TeamDto>(team);
        }
    }
}
