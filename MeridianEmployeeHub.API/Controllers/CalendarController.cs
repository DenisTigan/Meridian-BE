using System.Security.Claims;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Calendar;
using MeridianEmployeeHub.Services.Calendar.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/calendar/events")]
    [Authorize] // Toate rutele necesită autentificare
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarEventService _calendarService;

        public CalendarController(ICalendarEventService calendarService)
        {
            _calendarService = calendarService;
        }

        // ── GET /api/v1/calendar/events ───────────────────────────────────────
        // Toți angajații autentificați.
        // ?from=&to= opționale — dacă lipsesc, default = luna curentă (decis în service).
        // ?category= opțional — filtrare pe enum EventCategory.
        // Filtrare cu suprapunere: EndDateTime >= from AND StartDateTime <= to.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetAll(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] EventCategory? category)
        {
            var result = await _calendarService.GetAllAsync(from, to, category);
            return Ok(result);
        }

        // ── GET /api/v1/calendar/events/{id} ─────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CalendarEventDto>> GetById(int id)
        {
            var result = await _calendarService.GetByIdAsync(id);

            if (result is null)
                return NotFound($"Calendar event with id {id} not found.");

            return Ok(result);
        }

        // ── POST /api/v1/calendar/events ──────────────────────────────────────
        // Doar Manager, HR sau Admin pot crea evenimente.
        [HttpPost]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<ActionResult<CalendarEventDto>> Create(
            [FromBody] CreateCalendarEventRequest request)
        {
            var createdBy = GetCurrentEmployeeId();
            var created = await _calendarService.CreateAsync(request, createdBy);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        // ── PUT /api/v1/calendar/events/{id} ──────────────────────────────────
        // [Authorize] fără policy — ownership check în service (creator sau Admin).
        // Identic cu AnnouncementsController.Update — același pattern.
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CalendarEventDto>> Update(
            int id, [FromBody] UpdateCalendarEventRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isAdmin = User.IsInRole("Admin");

            var updated = await _calendarService.UpdateAsync(id, request, currentUserId, isAdmin);
            return Ok(updated);
        }

        // ── DELETE /api/v1/calendar/events/{id} ───────────────────────────────
        // [Authorize] fără policy — ownership check în service (creator sau Admin).
        // Diferit față de AnnouncementsController.Delete (care era strict AdminOnly).
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isAdmin = User.IsInRole("Admin");

            await _calendarService.DeleteAsync(id, currentUserId, isAdmin);
            return NoContent();
        }

        // ── Helper: extrage ID-ul angajatului curent din JWT ──────────────────
        // Identic cu AnnouncementsController.GetCurrentEmployeeId()
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
