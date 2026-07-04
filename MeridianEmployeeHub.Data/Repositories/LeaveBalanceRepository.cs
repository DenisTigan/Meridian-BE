using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class LeaveBalanceRepository : ILeaveBalanceRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveBalanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveBalance?> GetByEmployeeYearAndTypeAsync(int employeeId, short year, LeaveType leaveType)
        {
            return await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.Year == year && lb.LeaveType == leaveType);
        }

        public async Task<IEnumerable<LeaveBalance>> GetBalancesAsync(int employeeId, short year)
        {
            return await _context.LeaveBalances
                .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                .ToListAsync();
        }

        public async Task AddAsync(LeaveBalance balance)
        {
            await _context.LeaveBalances.AddAsync(balance);
        }

        public async Task AddRangeAsync(IEnumerable<LeaveBalance> balances)
        {
            await _context.LeaveBalances.AddRangeAsync(balances);
        }

        public async Task UpdateAsync(LeaveBalance balance)
        {
            _context.LeaveBalances.Update(balance);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
