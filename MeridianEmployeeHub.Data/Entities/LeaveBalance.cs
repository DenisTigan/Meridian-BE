namespace MeridianEmployeeHub.Data.Entities
{
    // O înregistrare pentru un angajat, pe un an specific, pentru un tip de concediu
    public class LeaveBalance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public short Year { get; set; }

        public LeaveType LeaveType { get; set; }

        public decimal AllottedDays { get; set; }
        public decimal UsedDays { get; set; }

        // RemainingDays nu e stocat în DB. Este calculat în DTO (AllottedDays - UsedDays).
    }
}
