namespace MeridianEmployeeHub.Data.Entities
{
    public class TrainingModule
    {
        public int Id { get; set; }
        
        public int CourseId { get; set; }
        public TrainingCourse? Course { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        
        public TrainingModuleType ModuleType { get; set; }
        public byte OrderIndex { get; set; }
        public int DurationMinutes { get; set; }
    }
}
