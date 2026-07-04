using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Services.Training.DTOs
{
    public class TrainingModuleDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public TrainingModuleType ModuleType { get; set; }
        public byte OrderIndex { get; set; }
        public int DurationMinutes { get; set; }
    }
}
