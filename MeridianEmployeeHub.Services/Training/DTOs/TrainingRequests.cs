using System.ComponentModel.DataAnnotations;
using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Training.DTOs
{
    public class CreateCourseRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TrainingCategory Category { get; set; }
        
        [Range(1, 10000)]
        public int EstimatedMinutes { get; set; }
        
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }
        
        public bool IsMandatoryForNewHires { get; set; }
    }

    public class UpdateCourseRequest : CreateCourseRequest
    {
        public bool IsActive { get; set; }
    }

    public class CreateModuleRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public TrainingModuleType ModuleType { get; set; }
        
        [Range(0, 255)]
        public byte OrderIndex { get; set; }
        
        [Range(1, 10000)]
        public int DurationMinutes { get; set; }
    }

    public class UpdateProgressRequest
    {
        [Range(0, 100)]
        public byte ProgressPercent { get; set; }
    }

    public class CreateEnrollmentRequest
    {
        [Required]
        public int CourseId { get; set; }
    }
}
