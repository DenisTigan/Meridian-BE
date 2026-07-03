using MeridianEmployeeHub.Services.DeskBookings;
using MeridianEmployeeHub.Services.DeskBookings.DTOs;
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
        private readonly IDeskBookingService _bookingService;

        // IDeskBookingService injectat pentru ?date= disponibilitate și /presence
        public DesksController(IDeskService deskService, IDeskBookingService bookingService)
        {
            _deskService = deskService;
            _bookingService = bookingService;
        }

        // ── GET /api/v1/desks ─────────────────────────────────────────────────
        // Fără ?date=: listă simplă de deskuri (comportament original din 8a).
        // Cu ?date=: răspuns extins cu IsAvailable per desk pentru data dată.
        // ?officeId= opțional — filtrează la un singur birou.
        //
        // IMPORTANT: această rută trebuie declarată ÎNAINTEA rutelor cu segmente statice
        // ("/presence") pentru a evita ambiguitate de routing ASP.NET Core.
        [HttpGet]
        public async Task<IActionResult> GetDesks(
            [FromQuery] DateOnly? date,
            [FromQuery] int? officeId)
        {
            if (date.HasValue)
            {
                // Răspuns extins cu disponibilitate
                var availability = await _bookingService.GetDeskAvailabilityAsync(
                    officeId, date.Value);
                return Ok(availability);
            }

            // Comportament original: listă simplă de deskuri per office
            if (officeId.HasValue)
            {
                var desks = await _deskService.GetDesksByOfficeAsync(officeId.Value);
                return Ok(desks);
            }

            // Fără niciun filtru: returnează toate deskurile active
            // (util pentru pagini de overview, nu cel mai frecvent caz)
            var allActive = await _deskService.GetAllActiveDesksAsync();
            return Ok(allActive);
        }

        // ── GET /api/v1/desks/presence ────────────────────────────────────────
        // Cine e în birou azi (sau la data cerută).
        // ?date= opțional — implicit data curentă.
        // ?departmentId= opțional — filtrare pe departament.
        //
        // IMPORTANT: declarată cu ruta explicită "presence" — trebuie să fie
        // ÎNAINTEA rutelor cu {id:int} pentru a nu fi confundată cu un ID numeric.
        [HttpGet("presence")]
        public async Task<ActionResult<IEnumerable<PresenceDto>>> GetPresence(
            [FromQuery] DateOnly? date,
            [FromQuery] int? departmentId)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await _bookingService.GetPresenceAsync(targetDate, departmentId);
            return Ok(result);
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
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeactivateDesk(int id)
        {
            await _deskService.DeactivateDeskAsync(id);
            return NoContent();
        }
    }
}
