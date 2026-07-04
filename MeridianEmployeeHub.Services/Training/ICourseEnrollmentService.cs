using MeridianEmployeeHub.Services.Training.DTOs;

namespace MeridianEmployeeHub.Services.Training
{
    public interface ICourseEnrollmentService
    {
        Task<CourseEnrollmentDto> EnrollEmployeeAsync(int courseId, int employeeId);
        Task<CourseEnrollmentDto> UpdateProgressAsync(int enrollmentId, int employeeId, byte progressPercent);
        
        Task<IEnumerable<CourseEnrollmentDto>> GetMyEnrollmentsAsync(int employeeId);
        Task<IEnumerable<CourseEnrollmentDto>> GetAllEnrollmentsAsync();
        
        Task<object> GetCertificateStatusAsync(int enrollmentId, int employeeId);
    }
}
