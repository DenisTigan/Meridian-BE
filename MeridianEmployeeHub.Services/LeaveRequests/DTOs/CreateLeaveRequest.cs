using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.LeaveRequests.DTOs
{
    public class CreateLeaveRequest
    {
        public LeaveType LeaveType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
