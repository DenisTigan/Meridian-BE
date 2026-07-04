using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ReviewedBy)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetRequestsAsync(int? employeeId = null, int? managerId = null)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ReviewedBy)
                .AsQueryable();

            if (employeeId.HasValue && managerId.HasValue)
            {
                query = query.Where(lr => lr.EmployeeId == employeeId.Value || lr.Employee.ManagerId == managerId.Value);
            }
            else if (employeeId.HasValue)
            {
                query = query.Where(lr => lr.EmployeeId == employeeId.Value);
            }
            else if (managerId.HasValue)
            {
                query = query.Where(lr => lr.Employee.ManagerId == managerId.Value);
            }

            return await query.OrderByDescending(lr => lr.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(LeaveRequest request)
        {
            await _context.LeaveRequests.AddAsync(request);
        }

        public async Task UpdateAsync(LeaveRequest request)
        {
            _context.LeaveRequests.Update(request);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
