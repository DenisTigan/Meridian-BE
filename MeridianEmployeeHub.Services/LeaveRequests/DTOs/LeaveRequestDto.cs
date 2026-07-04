using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.LeaveRequests.DTOs
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        
        public LeaveType LeaveType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public string? Reason { get; set; }
        
        public LeaveRequestStatus Status { get; set; }
        public int? ReviewedById { get; set; }
        public string? ReviewedByName { get; set; }
        public string? ManagerComment { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
