using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Training.DTOs
{
    public class TrainingCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TrainingCategory Category { get; set; }
        public int EstimatedMinutes { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsMandatoryForNewHires { get; set; }
        
        public int CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<TrainingModuleDto> Modules { get; set; } = new List<TrainingModuleDto>();
    }
}
