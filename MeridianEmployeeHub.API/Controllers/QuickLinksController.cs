using MeridianEmployeeHub.Services.QuickLinks;
using MeridianEmployeeHub.Services.QuickLinks.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/quick-links")]
    [Authorize] // Toate rutele necesită autentificare
    public class QuickLinksController : ControllerBase
    {
        private readonly IQuickLinkService _quickLinkService;

        public QuickLinksController(IQuickLinkService quickLinkService)
        {
            _quickLinkService = quickLinkService;
        }

        // ── GET /api/v1/quick-links ───────────────────────────────────────────
        // Toți angajații autentificați — doar IsActive=true, sortate Category → OrderIndex.
        // Răspuns: listă plată (frontend grupează vizual pe baza câmpului Category).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuickLinkDto>>> GetAll()
        {
            var result = await _quickLinkService.GetAllActiveAsync();
            return Ok(result);
        }

        // ── POST /api/v1/quick-links ─────────────────────────────────────────
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<QuickLinkDto>> Create(
            [FromBody] CreateQuickLinkRequest request)
        {
            var created = await _quickLinkService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetAll),
                new { id = created.Id },
                created);
        }

        // ── PUT /api/v1/quick-links/{id} ─────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<QuickLinkDto>> Update(
            int id, [FromBody] UpdateQuickLinkRequest request)
        {
            var updated = await _quickLinkService.UpdateAsync(id, request);
            return Ok(updated);
        }

        // ── DELETE /api/v1/quick-links/{id} ──────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            await _quickLinkService.DeleteAsync(id);
            return NoContent();
        }

        // ── PATCH /api/v1/quick-links/reorder ────────────────────────────────
        // Body: { "orderedIds": [3, 1, 4, 2] }
        // Setează OrderIndex = index_în_array pentru fiecare ID.
        // ID-uri necunoscute sunt ignorate silențios.
        // IMPORTANT: declarată explicit cu ruta "reorder" pentru a evita conflictul cu {id}
        [HttpPatch("reorder")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Reorder([FromBody] ReorderQuickLinksRequest request)
        {
            await _quickLinkService.ReorderAsync(request);
            return NoContent();
        }
    }
}
