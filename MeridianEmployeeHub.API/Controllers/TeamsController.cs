using MeridianEmployeeHub.Services.Teams;
using MeridianEmployeeHub.Services.Teams.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/teams")]
    [Authorize] // Toate rutele necesită autentificare
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        // ── GET /api/v1/teams ─────────────────────────────────────────────────
        // Toți utilizatorii autentificați pot vizualiza lista de echipe.
        // Suportă: ?departmentId= pentru filtrare după departament.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetAllTeams(
            [FromQuery] int? departmentId)
        {
            IEnumerable<TeamDto> teams;

            if (departmentId.HasValue)
                teams = await _teamService.GetByDepartmentAsync(departmentId.Value);
            else
                teams = await _teamService.GetAllTeamsAsync();

            return Ok(teams);
        }

        // ── GET /api/v1/teams/{id} ────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TeamDto>> GetTeamById(int id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);

            if (team == null)
                return NotFound();

            return Ok(team);
        }

        // ── POST /api/v1/teams ────────────────────────────────────────────────
        // Exclusiv Admin.
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<TeamDto>> CreateTeam(
            [FromBody] CreateTeamRequest request)
        {
            var newTeam = await _teamService.CreateTeamAsync(request);

            return CreatedAtAction(
                nameof(GetTeamById),
                new { id = newTeam.Id },
                newTeam);
        }

        // ── PUT /api/v1/teams/{id} ────────────────────────────────────────────
        // Manager, HR sau Admin pot actualiza o echipă.
        [HttpPut("{id:int}")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<ActionResult<TeamDto>> UpdateTeam(
            int id, [FromBody] UpdateTeamRequest request)
        {
            var updated = await _teamService.UpdateTeamAsync(id, request);
            return Ok(updated);
        }

        // ── DELETE /api/v1/teams/{id} ─────────────────────────────────────────
        // Exclusiv Admin.
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            await _teamService.DeleteTeamAsync(id);
            return NoContent(); // 204 No Content
        }
    }
}
