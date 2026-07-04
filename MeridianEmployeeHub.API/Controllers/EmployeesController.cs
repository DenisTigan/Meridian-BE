using System.Security.Claims;
using MeridianEmployeeHub.Services.Employees;
using MeridianEmployeeHub.Services.Employees.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/employees")]
    [Authorize] // Toate rutele necesită autentificare — excepțiile sunt marcate explicit
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IProfilePictureService _profilePictureService;

        public EmployeesController(
            IEmployeeService employeeService,
            IProfilePictureService profilePictureService)
        {
            _employeeService = employeeService;
            _profilePictureService = profilePictureService;
        }

        // ── GET /api/v1/employees ─────────────────────────────────────────────
        // Toți utilizatorii autentificați pot accesa lista.
        // Suportă: ?search= &departmentId= &teamId= &page= &pageSize=
        [HttpGet]
        public async Task<ActionResult<PagedEmployeeResponse>> GetAllEmployees(
            [FromQuery] string? search,
            [FromQuery] int? departmentId,
            [FromQuery] int? teamId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _employeeService.GetAllEmployeesAsync(
                search, departmentId, teamId, page, pageSize);
            return Ok(result);
        }

        // ── GET /api/v1/employees/me ──────────────────────────────────────────
        // Profilul propriu al utilizatorului curent.
        // IMPORTANT: ruta "me" trebuie declarată ÎNAINTEA "{id}" pentru a nu fi interceptată de aceasta.
        [HttpGet("me")]
        public async Task<ActionResult<EmployeeDto>> GetMyProfile()
        {
            var currentUserId = GetCurrentEmployeeId();
            var employee = await _employeeService.GetCurrentUserProfileAsync(currentUserId);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // ── GET /api/v1/employees/{id} ────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // ── POST /api/v1/employees ────────────────────────────────────────────
        // Doar HR sau Admin pot crea angajați noi.
        [HttpPost]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(
            [FromBody] CreateEmployeeRequest request)
        {
            var newEmployee = await _employeeService.CreateEmployeeAsync(request);

            // 201 Created cu Location header care pointează la GET /{id}
            return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee);
        }

        // ── PUT /api/v1/employees/{id} ────────────────────────────────────────
        // Orice angajat autentificat poate încerca; ownership check-ul e în service.
        // Un angajat obișnuit poate edita doar propriile câmpuri (PhoneNumber, ProfilePictureUrl).
        // HR/Admin pot edita orice câmp pe orice angajat.
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(
            int id, [FromBody] UpdateEmployeeRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = IsHROrAdmin();

            var updated = await _employeeService.UpdateEmployeeAsync(
                id, request, currentUserId, isHROrAdmin);

            return Ok(updated);
        }

        // ── DELETE /api/v1/employees/{id} ─────────────────────────────────────
        // Soft-delete: IsActive = false. Exclusiv Admin.
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            await _employeeService.DeactivateEmployeeAsync(id);
            return NoContent(); // 204 No Content
        }

        // ── PATCH /api/v1/employees/{id}/work-status ──────────────────────────
        // Self-only: un angajat poate schimba DOAR propriul WorkStatus.
        // Ownership check-ul este aplicat în service (aruncă ForbiddenException dacă nu e self).
        [HttpPatch("{id:int}/work-status")]
        public async Task<ActionResult<EmployeeDto>> UpdateWorkStatus(
            int id, [FromBody] UpdateWorkStatusRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();

            var updated = await _employeeService.UpdateWorkStatusAsync(
                id, request.WorkStatus, currentUserId);

            return Ok(updated);
        }

        // ── PUT /api/v1/employees/{id}/picture ────────────────────────────────
        // Upload sau înlocuire poză de profil.
        // Acceptă multipart/form-data cu un câmp "file".
        [HttpPut("{id:int}/picture")]
        public async Task<ActionResult> UploadProfilePicture(int id, IFormFile file)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = IsHROrAdmin();

            if (currentUserId != id && !isHROrAdmin)
                throw new MeridianEmployeeHub.Services.Exceptions.ForbiddenException(
                    "You are not allowed to update another employee's profile picture.");

            var newUrl = await _profilePictureService.UploadProfilePictureAsync(id, file);
            return Ok(new { ProfilePictureUrl = newUrl });
        }

        // ── DELETE /api/v1/employees/{id}/picture ─────────────────────────────
        // Șterge poza de profil.
        [HttpDelete("{id:int}/picture")]
        public async Task<IActionResult> DeleteProfilePicture(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = IsHROrAdmin();

            if (currentUserId != id && !isHROrAdmin)
                throw new MeridianEmployeeHub.Services.Exceptions.ForbiddenException(
                    "You are not allowed to delete another employee's profile picture.");

            await _profilePictureService.DeleteProfilePictureAsync(id);
            return NoContent();
        }

        // ── GET /api/v1/employees/{id}/badge ───────────────────────────────────
        // Badge virtual - Doar angajatul insusi sau Admin pot accesa.
        [HttpGet("{id:int}/badge")]
        public async Task<ActionResult<BadgeDto>> GetBadgeData(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isAdmin = User.IsInRole("Admin");

            var badgeData = await _employeeService.GetBadgeDataAsync(id, currentUserId, isAdmin);
            if (badgeData == null)
            {
                return NotFound();
            }

            return Ok(badgeData);
        }

        // ── Helper methods ────────────────────────────────────────────────────

        private int GetCurrentEmployeeId()
        {
            // JWT emis de AuthService conține claim-ul "sub" (NameIdentifier) cu ID-ul angajatului
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException(
                    "Invalid token: missing or invalid subject claim.");

            return employeeId;
        }

        private bool IsHROrAdmin()
        {
            // Citim rolul din claim-ul JWT — NU din body-ul request-ului
            return User.IsInRole("HR") || User.IsInRole("Admin");
        }
    }
}