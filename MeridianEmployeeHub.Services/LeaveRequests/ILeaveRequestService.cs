using MeridianEmployeeHub.Services.LeaveRequests.DTOs;

namespace MeridianEmployeeHub.Services.LeaveRequests
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequestDto>> GetRequestsAsync(int currentUserId, bool isHROrAdmin, bool isManager);
        Task<LeaveRequestDto?> GetRequestByIdAsync(int id, int currentUserId, bool isHROrAdmin);
        Task<LeaveRequestDto> CreateRequestAsync(int currentUserId, CreateLeaveRequest request);
        Task<LeaveRequestDto> ReviewRequestAsync(int id, int reviewerId, ReviewLeaveRequest review);
    }
}
