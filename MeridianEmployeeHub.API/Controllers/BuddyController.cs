using System.Security.Claims;
using MeridianEmployeeHub.Services.Buddy;
using MeridianEmployeeHub.Services.Buddy.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/buddy")]
    [Authorize] // Toate rutele necesită autentificare
    public class BuddyController : ControllerBase
    {
        private readonly IBuddyService _buddyService;

        public BuddyController(IBuddyService buddyService)
        {
            _buddyService = buddyService;
        }

        // ── GET /api/v1/buddy/my-assignment ───────────────────────────────────
        // Returnează assignment-ul activ propriu (ca new employee).
        // 204 No Content dacă nu există — absența e stare normală, nu eroare.
        // IMPORTANT: declarată înaintea rutelor cu {id} pentru a nu fi interceptată.
        [HttpGet("my-assignment")]
        public async Task<ActionResult<BuddyAssignmentDto>> GetMyAssignment()
        {
            var currentUserId = GetCurrentEmployeeId();
            var result = await _buddyService.GetActiveAssignmentForEmployeeAsync(currentUserId);

            if (result is null)
                return NoContent(); // 204 — fără buddy asignat, stare normală

            return Ok(result);
        }

        // ── GET /api/v1/buddy/assignments ─────────────────────────────────────
        // Lista completă a tuturor assignment-urilor — doar HR/Admin.
        [HttpGet("assignments")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<IEnumerable<BuddyAssignmentDto>>> GetAllAssignments()
        {
            var result = await _buddyService.GetAllAssignmentsAsync();
            return Ok(result);
        }

        // ── POST /api/v1/buddy/assignments ────────────────────────────────────
        // Creează un assignment nou — doar HR/Admin.
        // 409 Conflict dacă NewEmployee are deja un assignment activ.
        [HttpPost("assignments")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<BuddyAssignmentDto>> AssignBuddy(
            [FromBody] AssignBuddyRequest request)
        {
            var created = await _buddyService.AssignBuddyAsync(request);

            // 201 Created cu Location header
            return CreatedAtAction(
                nameof(GetAllAssignments),
                new { id = created.Id },
                created);
        }

        // ── PUT /api/v1/buddy/assignments/{id} ────────────────────────────────
        // Actualizează buddy-ul și/sau notele — doar HR/Admin.
        [HttpPut("assignments/{id:int}")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<BuddyAssignmentDto>> UpdateAssignment(
            int id, [FromBody] UpdateBuddyAssignmentRequest request)
        {
            var updated = await _buddyService.UpdateAssignmentAsync(id, request);
            return Ok(updated);
        }

        // ── PATCH /api/v1/buddy/assignments/{id}/complete ─────────────────────
        // Marchează assignment-ul ca Completed — doar HR/Admin.
        [HttpPatch("assignments/{id:int}/complete")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<BuddyAssignmentDto>> CompleteAssignment(int id)
        {
            var completed = await _buddyService.CompleteAssignmentAsync(id);
            return Ok(completed);
        }

        // ── Helper methods ────────────────────────────────────────────────────

        private int GetCurrentEmployeeId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException(
                    "Invalid token: missing or invalid subject claim.");

            return employeeId;
        }
    }
}
