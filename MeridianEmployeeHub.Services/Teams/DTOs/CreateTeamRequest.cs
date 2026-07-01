namespace MeridianEmployeeHub.Services.Teams.DTOs
{
    public class CreateTeamRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public int? TeamLeadId { get; set; }
    }
}
