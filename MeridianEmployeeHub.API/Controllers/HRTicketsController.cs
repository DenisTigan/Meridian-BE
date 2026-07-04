using System.Security.Claims;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.HRTickets;
using MeridianEmployeeHub.Services.HRTickets.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/hr/tickets")]
    [Authorize] // Toate rutele necesită autentificare
    public class HRTicketsController : ControllerBase
    {
        private readonly IHRTicketService _ticketService;

        public HRTicketsController(IHRTicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // ── GET /api/v1/hr/tickets ─────────────────────────────────────────────
        // Comportament dual — logica în service, NU în controller:
        //   Angajat obișnuit: vede doar tichetele proprii
        //   HR/Admin:         vede toate tichetele
        // Filtre opționale: ?category= &status=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HRTicketDto>>> GetTickets(
            [FromQuery] TicketCategory? category,
            [FromQuery] TicketStatus? status)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = IsHROrAdmin();

            var result = await _ticketService.GetTicketsAsync(
                currentUserId, isHROrAdmin, category, status);

            return Ok(result);
        }

        // ── GET /api/v1/hr/tickets/{id} ────────────────────────────────────────
        // Ownership check în service:
        //   - Angajatul propriu SAU HR/Admin → 200 OK
        //   - Altul → ForbiddenException (403) — tichetul există, dar nu ai voie să-l vezi
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HRTicketDto>> GetById(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = IsHROrAdmin();

            var result = await _ticketService.GetByIdAsync(id, currentUserId, isHROrAdmin);
            return Ok(result);
        }

        // ── POST /api/v1/hr/tickets ────────────────────────────────────────────
        // Orice angajat autentificat poate depune un tichet
        [HttpPost]
        public async Task<ActionResult<HRTicketDto>> Create([FromBody] CreateTicketRequest request)
        {
            var employeeId = GetCurrentEmployeeId();
            var created = await _ticketService.CreateAsync(request, employeeId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        // ── PATCH /api/v1/hr/tickets/{id}/status ──────────────────────────────
        // Exclusiv HR sau Admin.
        // ResolvedAt setat/resetat automat în funcție de noul status (logica în service).
        [HttpPatch("{id:int}/status")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<HRTicketDto>> UpdateStatus(
            int id, [FromBody] UpdateTicketStatusRequest request)
        {
            var updated = await _ticketService.UpdateStatusAsync(id, request);
            return Ok(updated);
        }

        // ── PATCH /api/v1/hr/tickets/{id}/assign ──────────────────────────────
        // Exclusiv HR sau Admin.
        // Body: { "assignedToId": int }
        [HttpPatch("{id:int}/assign")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<HRTicketDto>> Assign(
            int id, [FromBody] AssignTicketRequest request)
        {
            var updated = await _ticketService.AssignAsync(id, request);
            return Ok(updated);
        }

        // ── Helper: extrage ID-ul angajatului curent din JWT ──────────────────
        // Identic cu CalendarController.GetCurrentEmployeeId()
        private int GetCurrentEmployeeId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException(
                    "Invalid token: missing or invalid subject claim.");

            return employeeId;
        }

        private bool IsHROrAdmin()
        {
            return User.IsInRole("HR") || User.IsInRole("Admin");
        }
    }
}
