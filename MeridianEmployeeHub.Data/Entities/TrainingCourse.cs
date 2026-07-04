namespace MeridianEmployeeHub.Data.Entities
{
    public class TrainingCourse
    {
        public int Id { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TrainingCategory Category { get; set; }
        
        public int EstimatedMinutes { get; set; }
        public string? ThumbnailUrl { get; set; }
        
        public bool IsMandatoryForNewHires { get; set; }
        public bool IsActive { get; set; } = true;
        
        public int CreatedById { get; set; }
        public Employee? CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<TrainingModule> Modules { get; set; } = new List<TrainingModule>();
        public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
    }
}
