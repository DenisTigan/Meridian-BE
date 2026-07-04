namespace MeridianEmployeeHub.Data.Entities
{
    // LeaveRequest nu moștenește BaseEntity — entitate de tranzacție de HR.
    public class LeaveRequest
    {
        public int Id { get; set; }

        // FK NOT NULL → Employees, DeleteBehavior.Restrict
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public LeaveType LeaveType { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // Zile lucrătoare, calculate automat de service la creare/modificare date.
        public decimal TotalDays { get; set; }

        public string? Reason { get; set; }

        // Status curent — default Pending
        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        // FK nullable → Employees (managerul sau HR care a aprobat/respins)
        public int? ReviewedById { get; set; }
        public Employee? ReviewedBy { get; set; }

        public string? ManagerComment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
