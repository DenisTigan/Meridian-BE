using System.Security.Claims;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Announcements;
using MeridianEmployeeHub.Services.Announcements.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/announcements")]
    [Authorize] // Toate rutele necesită autentificare
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // ── GET /api/v1/announcements ─────────────────────────────────────────
        // Toți angajații autentificați; filtru opțional pe categorie; paginat.
        // Vizibilitate: HR/Admin văd tot; restul văd doar IsPublished=true +
        //               propriile anunțuri nepublicate.
        [HttpGet]
        public async Task<ActionResult<PagedAnnouncementResponse>> GetAll(
            [FromQuery] AnnouncementCategory? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isPrivileged = IsHROrAdmin();

            var result = await _announcementService.GetAllAsync(
                category, isPrivileged, currentUserId, page, pageSize);

            return Ok(result);
        }

        // ── GET /api/v1/announcements/{id} ────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnnouncementDto>> GetById(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isPrivileged = IsHROrAdmin();

            var result = await _announcementService.GetByIdAsync(id, isPrivileged, currentUserId);

            if (result is null)
                return NotFound($"Announcement with id {id} not found.");

            return Ok(result);
        }

        // ── POST /api/v1/announcements ────────────────────────────────────────
        // Doar HR sau Admin pot crea anunțuri.
        [HttpPost]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<AnnouncementDto>> Create(
            [FromBody] CreateAnnouncementRequest request)
        {
            var authorId = GetCurrentEmployeeId();
            var created = await _announcementService.CreateAsync(request, authorId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        // ── PUT /api/v1/announcements/{id} ────────────────────────────────────
        // [Authorize] fără policy — ownership check se face în service (author sau admin)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AnnouncementDto>> Update(
            int id, [FromBody] UpdateAnnouncementRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isAdmin = User.IsInRole("Admin");

            var updated = await _announcementService.UpdateAsync(id, request, currentUserId, isAdmin);
            return Ok(updated);
        }

        // ── DELETE /api/v1/announcements/{id} ────────────────────────────────
        // Strict Admin — autorul NU poate șterge propriul anunț (conform specificației)
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            await _announcementService.DeleteAsync(id);
            return NoContent();
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

        // HR sau Admin au vizibilitate completă (văd anunțuri nepublicate)
        private bool IsHROrAdmin()
        {
            return User.IsInRole("HR") || User.IsInRole("Admin");
        }
    }
}
