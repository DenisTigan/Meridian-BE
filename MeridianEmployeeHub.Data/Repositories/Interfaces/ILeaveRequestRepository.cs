using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest?> GetByIdAsync(int id);

        // Fetching with dual behavior:
        // isHROrAdmin = true -> employeeId=null (all), or specific employeeId if filtering
        // managerId -> team's requests
        // regular employee -> own requests
        Task<IEnumerable<LeaveRequest>> GetRequestsAsync(int? employeeId = null, int? managerId = null);

        Task AddAsync(LeaveRequest request);
        Task UpdateAsync(LeaveRequest request);
        Task SaveChangesAsync();
    }
}
