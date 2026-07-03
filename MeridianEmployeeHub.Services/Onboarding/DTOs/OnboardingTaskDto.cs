using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Onboarding.DTOs
{
    public class OnboardingTaskDto
    {
        public int Id { get; set; }
        public int ChecklistId { get; set; }
        public OnboardingPhase Phase { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Expus pentru ca UI-ul să știe ce task-uri sunt auto-trigger
        public string? AutoTriggerType { get; set; }

        public byte OrderIndex { get; set; }
    }
}
