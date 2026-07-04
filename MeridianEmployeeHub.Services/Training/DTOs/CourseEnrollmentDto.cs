namespace MeridianEmployeeHub.Services.Training.DTOs
{
    public class CourseEnrollmentDto
    {
        public int Id { get; set; }
        
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public byte ProgressPercent { get; set; }
        public bool IsCompleted { get; set; }
        
        public string? CertificateUrl { get; set; }
    }
}
