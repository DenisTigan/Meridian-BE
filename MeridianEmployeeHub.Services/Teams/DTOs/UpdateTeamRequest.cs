namespace MeridianEmployeeHub.Services.Teams.DTOs
{
    // DTO pentru operatia PUT /api/v1/teams/{id}
    // Toate câmpurile sunt opționale (null = nu se modifică).
    public class UpdateTeamRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? DepartmentId { get; set; }
        public int? TeamLeadId { get; set; }
    }
}
