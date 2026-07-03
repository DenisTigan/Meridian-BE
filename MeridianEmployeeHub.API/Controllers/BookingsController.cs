using System.Security.Claims;
using MeridianEmployeeHub.Services.DeskBookings;
using MeridianEmployeeHub.Services.DeskBookings.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/bookings")]
    [Authorize] // Toate rutele necesită autentificare
    public class BookingsController : ControllerBase
    {
        private readonly IDeskBookingService _bookingService;

        public BookingsController(IDeskBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // ── GET /api/v1/bookings ──────────────────────────────────────────────
        // Rezervările proprii ale angajatului curent; paginat; filtru opțional pe interval.
        [HttpGet]
        public async Task<ActionResult<PagedBookingResponse>> GetMyBookings(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetCurrentEmployeeId();
            var result = await _bookingService.GetMyBookingsAsync(
                currentUserId, from, to, page, pageSize);

            return Ok(result);
        }

        // ── POST /api/v1/bookings ─────────────────────────────────────────────
        // Creare rezervare — verificare Strat 1 + Strat 2 (index UNIQUE) în service.
        // 409 Conflict dacă desk-ul e deja rezervat pentru data dată.
        [HttpPost]
        public async Task<ActionResult<DeskBookingDto>> CreateBooking(
            [FromBody] CreateBookingRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var created = await _bookingService.CreateBookingAsync(request, currentUserId);

            return CreatedAtAction(
                nameof(GetBookingsByDate),
                new { date = created.BookingDate },
                created);
        }

        // ── DELETE /api/v1/bookings/{id} ──────────────────────────────────────
        // Anulare rezervare — DOAR proprie (ownership check în service).
        // Soft: Status = Cancelled, CancelledAt = UtcNow, NU ștergere fizică.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            await _bookingService.CancelBookingAsync(id, currentUserId);
            return NoContent();
        }

        // ── GET /api/v1/bookings/date/{date} ──────────────────────────────────
        // Toate rezervările unei date — doar Manager, HR sau Admin.
        // IMPORTANT: declarată cu ruta explicită "date/{date}" — trebuie să fie
        // ÎNAINTEA rutei cu {id:int} pentru a evita conflictul de routing.
        [HttpGet("date/{date}")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<ActionResult<IEnumerable<DeskBookingDto>>> GetBookingsByDate(
            DateOnly date)
        {
            var result = await _bookingService.GetBookingsByDateAsync(date);
            return Ok(result);
        }

        // ── Helper: extrage ID-ul angajatului curent din JWT ──────────────────
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
