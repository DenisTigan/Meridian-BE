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

        public async Task<IEnumerable<TeamDto>> GetByDepartmentAsync(int departmentId)
        {
            var teams = await _repository.GetByDepartmentAsync(departmentId);
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

        public async Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamRequest request)
        {
            var team = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Team with id {id} not found.");

            // Aplicăm doar câmpurile care au valori (patch-like behavior în PUT)
            if (request.Name is not null)
                team.Name = request.Name;

            if (request.Description is not null)
                team.Description = request.Description;

            if (request.DepartmentId.HasValue)
                team.DepartmentId = request.DepartmentId.Value;

            if (request.TeamLeadId.HasValue)
                team.TeamLeadId = request.TeamLeadId.Value;

            await _repository.UpdateAsync(team);
            await _repository.SaveChangesAsync();

            return _mapper.Map<TeamDto>(team);
        }

        public async Task DeleteTeamAsync(int id)
        {
            var team = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Team with id {id} not found.");

            await _repository.DeleteAsync(team);
            await _repository.SaveChangesAsync();
        }
    }
}
