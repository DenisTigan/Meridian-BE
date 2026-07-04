using MeridianEmployeeHub.Services.LeaveRequests.DTOs;

namespace MeridianEmployeeHub.Services.LeaveRequests
{
    public interface ILeaveBalanceService
    {
        Task<IEnumerable<LeaveBalanceDto>> GetBalancesAsync(int employeeId, short year);
    }
}
