using System.Security.Claims;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Training;
using MeridianEmployeeHub.Services.Training.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ITrainingCourseService _courseService;

        public CoursesController(ITrainingCourseService courseService)
        {
            _courseService = courseService;
        }

        // GET /api/v1/courses
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TrainingCourseDto>>> GetCourses(
            [FromQuery] TrainingCategory? category,
            [FromQuery] string? search)
        {
            var courses = await _courseService.GetCoursesAsync(category, search);
            return Ok(courses);
        }

        // GET /api/v1/courses/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<TrainingCourseDto>> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return Ok(course);
        }

        // POST /api/v1/courses
        [HttpPost]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<TrainingCourseDto>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var course = await _courseService.CreateCourseAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
        }

        // PUT /api/v1/courses/{id}
        [HttpPut("{id:int}")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<TrainingCourseDto>> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
        {
            var course = await _courseService.UpdateCourseAsync(id, request);
            return Ok(course);
        }

        // DELETE /api/v1/courses/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            return NoContent();
        }

        // GET /api/v1/courses/{id}/modules
        [HttpGet("{id:int}/modules")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TrainingModuleDto>>> GetCourseModules(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return Ok(course.Modules);
        }

        // POST /api/v1/courses/{id}/modules
        [HttpPost("{id:int}/modules")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<TrainingModuleDto>> AddModule(int id, [FromBody] CreateModuleRequest request)
        {
            var module = await _courseService.AddModuleAsync(id, request);
            return StatusCode(StatusCodes.Status201Created, module);
        }

        // PUT /api/v1/courses/{courseId}/modules/{moduleId}
        [HttpPut("{courseId:int}/modules/{moduleId:int}")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<TrainingModuleDto>> UpdateModule(int courseId, int moduleId, [FromBody] CreateModuleRequest request)
        {
            var module = await _courseService.UpdateModuleAsync(courseId, moduleId, request);
            return Ok(module);
        }

        // DELETE /api/v1/courses/{courseId}/modules/{moduleId}
        [HttpDelete("{courseId:int}/modules/{moduleId:int}")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult> DeleteModule(int courseId, int moduleId)
        {
            await _courseService.DeleteModuleAsync(courseId, moduleId);
            return NoContent();
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
