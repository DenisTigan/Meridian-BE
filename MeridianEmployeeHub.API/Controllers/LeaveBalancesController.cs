using System.Security.Claims;
using MeridianEmployeeHub.Services.LeaveRequests;
using MeridianEmployeeHub.Services.LeaveRequests.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/hr/leave-balance")]
    [Authorize]
    public class LeaveBalancesController : ControllerBase
    {
        private readonly ILeaveBalanceService _balanceService;

        public LeaveBalancesController(ILeaveBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        // GET /api/v1/hr/leave-balance/me
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetMyBalances()
        {
            var currentUserId = GetCurrentEmployeeId();
            short currentYear = (short)DateTime.UtcNow.Year;
            
            var balances = await _balanceService.GetBalancesAsync(currentUserId, currentYear);
            return Ok(balances);
        }

        // GET /api/v1/hr/leave-balance/{employeeId}
        [HttpGet("{employeeId:int}")]
        [Authorize(Policy = "HROrAdmin")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetEmployeeBalances(int employeeId)
        {
            short currentYear = (short)DateTime.UtcNow.Year;
            
            var balances = await _balanceService.GetBalancesAsync(employeeId, currentYear);
            return Ok(balances);
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
