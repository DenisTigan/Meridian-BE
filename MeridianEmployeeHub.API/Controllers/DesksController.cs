using MeridianEmployeeHub.Services.Desks;
using MeridianEmployeeHub.Services.Desks.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/desks")]
    [Authorize] // Toate rutele necesită autentificare
    public class DesksController : ControllerBase
    {
        private readonly IDeskService _deskService;

        public DesksController(IDeskService deskService)
        {
            _deskService = deskService;
        }

        // ── POST /api/v1/desks ────────────────────────────────────────────────
        // Adaugă un desk nou la un office existent.
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<DeskDto>> CreateDesk(
            [FromBody] CreateDeskRequest request)
        {
            var created = await _deskService.CreateDeskAsync(request);

            return CreatedAtAction(
                nameof(GetDeskById),
                new { id = created.Id },
                created);
        }

        // ── GET /api/v1/desks/{id} ────────────────────────────────────────────
        // Intern — folosit de CreatedAtAction; nu expus explicit în spec dar necesar
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DeskDto>> GetDeskById(int id)
        {
            var desk = await _deskService.GetDeskByIdAsync(id);

            if (desk is null)
                return NotFound($"Desk with id {id} not found.");

            return Ok(desk);
        }

        // ── PUT /api/v1/desks/{id} ────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<DeskDto>> UpdateDesk(
            int id, [FromBody] UpdateDeskRequest request)
        {
            var updated = await _deskService.UpdateDeskAsync(id, request);
            return Ok(updated);
        }

        // ── DELETE /api/v1/desks/{id} ─────────────────────────────────────────
        // Soft-delete: setează IsActive = false. NU ștergere fizică.
        // Motivare: sesiunea 8b va lega DeskBookings de Desk prin FK; ștergerea
        // fizică ar rupe istoricul și ar genera erori de integritate referențială.
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeactivateDesk(int id)
        {
            await _deskService.DeactivateDeskAsync(id);
            return NoContent(); // 204 — soft-delete reușit
        }
    }
}
