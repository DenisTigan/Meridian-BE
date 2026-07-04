using System.Security.Claims;
using MeridianEmployeeHub.Services.LeaveRequests;
using MeridianEmployeeHub.Services.LeaveRequests.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/hr/leave-requests")]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _requestService;

        public LeaveRequestsController(ILeaveRequestService requestService)
        {
            _requestService = requestService;
        }

        // GET /api/v1/hr/leave-requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetLeaveRequests()
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = User.IsInRole("HR") || User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");

            var requests = await _requestService.GetRequestsAsync(currentUserId, isHROrAdmin, isManager);
            return Ok(requests);
        }

        // GET /api/v1/hr/leave-requests/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequestById(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var isHROrAdmin = User.IsInRole("HR") || User.IsInRole("Admin");

            var request = await _requestService.GetRequestByIdAsync(id, currentUserId, isHROrAdmin);
            
            if (request == null) return NotFound();

            return Ok(request);
        }

        // POST /api/v1/hr/leave-requests
        [HttpPost]
        public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest([FromBody] CreateLeaveRequest requestDto)
        {
            var currentUserId = GetCurrentEmployeeId();
            
            var request = await _requestService.CreateRequestAsync(currentUserId, requestDto);

            return CreatedAtAction(nameof(GetLeaveRequestById), new { id = request.Id }, request);
        }

        // PATCH /api/v1/hr/leave-requests/{id}
        [HttpPatch("{id:int}")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<ActionResult<LeaveRequestDto>> ReviewLeaveRequest(int id, [FromBody] ReviewLeaveRequest reviewDto)
        {
            var currentUserId = GetCurrentEmployeeId();

            var request = await _requestService.ReviewRequestAsync(id, currentUserId, reviewDto);

            return Ok(request);
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
