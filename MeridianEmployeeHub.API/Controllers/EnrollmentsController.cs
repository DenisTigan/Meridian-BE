using System.Security.Claims;
using MeridianEmployeeHub.Services.Training;
using MeridianEmployeeHub.Services.Training.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly ICourseEnrollmentService _enrollmentService;

        public EnrollmentsController(ICourseEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // GET /api/v1/enrollments/me
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CourseEnrollmentDto>>> GetMyEnrollments()
        {
            var currentUserId = GetCurrentEmployeeId();
            var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(currentUserId);
            return Ok(enrollments);
        }

        // POST /api/v1/enrollments
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CourseEnrollmentDto>> Enroll([FromBody] CreateEnrollmentRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var enrollment = await _enrollmentService.EnrollEmployeeAsync(request.CourseId, currentUserId);
            return StatusCode(StatusCodes.Status201Created, enrollment);
        }

        // PATCH /api/v1/enrollments/{id}/progress
        [HttpPatch("{id:int}/progress")]
        [Authorize]
        public async Task<ActionResult<CourseEnrollmentDto>> UpdateProgress(int id, [FromBody] UpdateProgressRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var enrollment = await _enrollmentService.UpdateProgressAsync(id, currentUserId, request.ProgressPercent);
            return Ok(enrollment);
        }

        // GET /api/v1/enrollments/{id}/certificate
        [HttpGet("{id:int}/certificate")]
        [Authorize]
        public async Task<ActionResult> GetCertificate(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var result = await _enrollmentService.GetCertificateStatusAsync(id, currentUserId);
            return Ok(result);
        }

        // GET /api/v1/enrollments
        [HttpGet]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<IEnumerable<CourseEnrollmentDto>>> GetAllEnrollments()
        {
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
            return Ok(enrollments);
        }

        private int GetCurrentEmployeeId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException("Invalid token: missing or invalid subject claim.");

            return employeeId;
        }
    }
}
