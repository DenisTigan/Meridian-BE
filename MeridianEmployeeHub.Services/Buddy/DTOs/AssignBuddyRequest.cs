namespace MeridianEmployeeHub.Services.Buddy.DTOs
{
    // Body pentru POST /api/v1/buddy/assignments
    public class AssignBuddyRequest
    {
        public int NewEmployeeId { get; set; }
        public int BuddyId { get; set; }
        public string? Notes { get; set; }
    }
}
