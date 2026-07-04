using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.LeaveRequests.DTOs;

namespace MeridianEmployeeHub.Services.LeaveRequests
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly ILeaveBalanceRepository _repository;

        public LeaveBalanceService(ILeaveBalanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LeaveBalanceDto>> GetBalancesAsync(int employeeId, short year)
        {
            var balances = await _repository.GetBalancesAsync(employeeId, year);

            return balances.Select(b => new LeaveBalanceDto
            {
                Id = b.Id,
                EmployeeId = b.EmployeeId,
                Year = b.Year,
                LeaveType = b.LeaveType,
                AllottedDays = b.AllottedDays,
                UsedDays = b.UsedDays
                // RemainingDays is computed in the DTO
            });
        }
    }
}
