namespace MeridianEmployeeHub.Services.Onboarding.DTOs
{
    public class OnboardingChecklistDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        // Progres calculat (0-100), returnat calculat la fiecare cerere
        public byte OverallProgress { get; set; }

        public DateTime CreatedAt { get; set; }

        // Task-uri sortate după OrderIndex (crescător)
        public IEnumerable<OnboardingTaskDto> Tasks { get; set; } = new List<OnboardingTaskDto>();
    }
}
