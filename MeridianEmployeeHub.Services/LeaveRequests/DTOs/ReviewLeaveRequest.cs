using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.LeaveRequests.DTOs
{
    public class ReviewLeaveRequest
    {
        // Status poate fi schimbat doar în Approved sau Rejected
        public LeaveRequestStatus Status { get; set; }
        public string? ManagerComment { get; set; }
    }
}
