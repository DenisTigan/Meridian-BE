using MeridianEmployeeHub.Services.Departments;
using MeridianEmployeeHub.Services.Departments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/departments")]
    [Authorize] // Toate rutele necesită autentificare
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // ── GET /api/v1/departments ───────────────────────────────────────────
        // Toți utilizatorii autentificați pot vizualiza lista de departamente.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAllDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        // ── GET /api/v1/departments/{id} ──────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartmentById(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);

            if (department == null)
                return NotFound();

            return Ok(department);
        }

        // ── POST /api/v1/departments ──────────────────────────────────────────
        // Exclusiv Admin.
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(
            [FromBody] CreateDepartmentRequest request)
        {
            var newDepartment = await _departmentService.CreateDepartmentAsync(request);

            return CreatedAtAction(
                nameof(GetDepartmentById),
                new { id = newDepartment.Id },
                newDepartment);
        }

        // ── PUT /api/v1/departments/{id} ──────────────────────────────────────
        // Exclusiv Admin.
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<DepartmentDto>> UpdateDepartment(
            int id, [FromBody] UpdateDepartmentRequest request)
        {
            var updated = await _departmentService.UpdateDepartmentAsync(id, request);
            return Ok(updated);
        }

        // ── DELETE /api/v1/departments/{id} ───────────────────────────────────
        // Exclusiv Admin. Ștergere fizică (Department nu are soft-delete).
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return NoContent(); // 204 No Content
        }
    }
}
