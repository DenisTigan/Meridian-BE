using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface ILeaveBalanceRepository
    {
        Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(int employeeId, short year, LeaveType leaveType);
        Task<IEnumerable<LeaveBalance>> GetBalancesAsync(int employeeId, short year);

        Task AddAsync(LeaveBalance balance);
        Task AddRangeAsync(IEnumerable<LeaveBalance> balances);
        Task UpdateAsync(LeaveBalance balance);
        Task SaveChangesAsync();
    }
}
