namespace MeridianEmployeeHub.Data.Entities
{
    public class CourseEnrollment
    {
        public int Id { get; set; }
        
        public int CourseId { get; set; }
        public TrainingCourse? Course { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime EnrolledAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public byte ProgressPercent { get; set; } // 0-100
        public bool IsCompleted { get; set; }
        
        public string? CertificateUrl { get; set; } // null for MVP
    }
}
