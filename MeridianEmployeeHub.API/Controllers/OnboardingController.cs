using System.Security.Claims;
using MeridianEmployeeHub.Services.Onboarding;
using MeridianEmployeeHub.Services.Onboarding.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/onboarding")]
    [Authorize] // Toate rutele necesită autentificare
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _onboardingService;

        public OnboardingController(IOnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
        }

        // ── GET /api/v1/onboarding/checklist ─────────────────────────────────
        // Returnează checklist-ul propriu al angajatului curent.
        // DIFERIT de restul modulelor: NU returnează 404 dacă nu există —
        // creează checklist-ul automat la prima cerere.
        [HttpGet("checklist")]
        public async Task<ActionResult<OnboardingChecklistDto>> GetMyChecklist()
        {
            var currentUserId = GetCurrentEmployeeId();
            var result = await _onboardingService.GetOrCreateChecklistAsync(currentUserId);
            return Ok(result);
        }

        // ── GET /api/v1/onboarding/checklist/{employeeId} ────────────────────
        // Returnează checklist-ul unui angajat specificat.
        // Disponibil DOAR pentru Manager, HR sau Admin.
        // Returnează 404 dacă angajatul specificat nu are încă un checklist.
        [HttpGet("checklist/{employeeId:int}")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<ActionResult<OnboardingChecklistDto>> GetChecklistByEmployeeId(int employeeId)
        {
            var result = await _onboardingService.GetChecklistByEmployeeIdAsync(employeeId);
            return Ok(result);
        }

        // ── PATCH /api/v1/onboarding/tasks/{id}/complete ─────────────────────
        // Marchează un task ca și completat.
        // Ownership check în service: task-ul trebuie să aparțină checklist-ului propriu.
        // Returnează 403 dacă task-ul aparține unui alt angajat.
        [HttpPatch("tasks/{id:int}/complete")]
        public async Task<ActionResult<OnboardingTaskDto>> CompleteTask(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var result = await _onboardingService.CompleteTaskAsync(id, currentUserId);
            return Ok(result);
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
    }
}
