using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Employees.DTOs;

namespace MeridianEmployeeHub.Services.Employees
{
    public interface IEmployeeService
    {
        // ── Interogare ────────────────────────────────────────────────────────
        Task<PagedEmployeeResponse> GetAllEmployeesAsync(
            string? search, int? departmentId, int? teamId, int page, int pageSize);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto?> GetCurrentUserProfileAsync(int currentUserId);

        // ── Creare ───────────────────────────────────────────────────────────
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request);

        // ── Actualizare ───────────────────────────────────────────────────────
        // isHROrAdmin: extrage din claims (role) în controller — nu din body request.
        // Dacă currentUserId != id ȘI !isHROrAdmin → aruncă ForbiddenException.
        Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, int currentUserId, bool isHROrAdmin);

        // Actualizează doar WorkStatus — self only (currentUserId == id) sau HR/Admin.
        Task<EmployeeDto> UpdateWorkStatusAsync(int id, WorkStatus newStatus, int currentUserId);

        // ── Dezactivare (soft-delete) ─────────────────────────────────────────
        Task DeactivateEmployeeAsync(int id);
    }
}