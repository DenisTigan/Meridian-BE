using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.LeaveRequests.DTOs
{
    public class LeaveBalanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public short Year { get; set; }
        public LeaveType LeaveType { get; set; }
        public decimal AllottedDays { get; set; }
        public decimal UsedDays { get; set; }

        // Computed property in DTO
        public decimal RemainingDays => AllottedDays - UsedDays;
    }
}
