using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Buddy.DTOs
{
    public class BuddyAssignmentDto
    {
        public int Id { get; set; }

        public int NewEmployeeId { get; set; }
        // Calculat de AutoMapper din NewEmployee.FirstName + " " + NewEmployee.LastName
        public string NewEmployeeFullName { get; set; } = string.Empty;

        public int BuddyId { get; set; }
        // Calculat de AutoMapper din Buddy.FirstName + " " + Buddy.LastName
        public string BuddyFullName { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; }
        public string? Notes { get; set; }
        public BuddyStatus Status { get; set; }
    }
}
