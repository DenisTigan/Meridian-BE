using MeridianEmployeeHub.Services.Desks;
using MeridianEmployeeHub.Services.Desks.DTOs;
using MeridianEmployeeHub.Services.Offices;
using MeridianEmployeeHub.Services.Offices.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/offices")]
    [Authorize] // Toate rutele necesită autentificare
    public class OfficesController : ControllerBase
    {
        private readonly IOfficeService _officeService;
        private readonly IDeskService _deskService;

        // IDeskService injectat direct în OfficesController pentru ruta nested:
        // GET /offices/{officeId}/desks — evită crearea unui controller separat pentru această rută
        public OfficesController(IOfficeService officeService, IDeskService deskService)
        {
            _officeService = officeService;
            _deskService = deskService;
        }

        // ── GET /api/v1/offices ───────────────────────────────────────────────
        // Toți angajații autentificați; TotalDesks calculat dinamic.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfficeDto>>> GetAllOffices()
        {
            var offices = await _officeService.GetAllOfficesAsync();
            return Ok(offices);
        }

        // ── GET /api/v1/offices/{id} ──────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OfficeDto>> GetOfficeById(int id)
        {
            var office = await _officeService.GetOfficeByIdAsync(id);

            if (office is null)
                return NotFound($"Office with id {id} not found.");

            return Ok(office);
        }

        // ── GET /api/v1/offices/{officeId}/desks ─────────────────────────────
        // Returnează toate desk-urile (inclusiv inactive) — Admin poate filtra vizual
        [HttpGet("{officeId:int}/desks")]
        public async Task<ActionResult<IEnumerable<DeskDto>>> GetDesksByOffice(int officeId)
        {
            var desks = await _deskService.GetDesksByOfficeAsync(officeId);
            return Ok(desks);
        }

        // ── POST /api/v1/offices ──────────────────────────────────────────────
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<OfficeDto>> CreateOffice(
            [FromBody] CreateOfficeRequest request)
        {
            var created = await _officeService.CreateOfficeAsync(request);

            return CreatedAtAction(
                nameof(GetOfficeById),
                new { id = created.Id },
                created);
        }

        // ── PUT /api/v1/offices/{id} ──────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<OfficeDto>> UpdateOffice(
            int id, [FromBody] UpdateOfficeRequest request)
        {
            var updated = await _officeService.UpdateOfficeAsync(id, request);
            return Ok(updated);
        }

        // ── DELETE /api/v1/offices/{id} ───────────────────────────────────────
        // Ștergere fizică — dar blocată dacă office-ul mai are desk-uri active (verificat în service)
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteOffice(int id)
        {
            await _officeService.DeleteOfficeAsync(id);
            return NoContent();
        }
    }
}
