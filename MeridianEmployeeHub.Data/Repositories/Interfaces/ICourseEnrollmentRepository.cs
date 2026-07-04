using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface ICourseEnrollmentRepository
    {
        Task<CourseEnrollment?> GetByIdAsync(int id);
        Task<CourseEnrollment?> GetByCourseAndEmployeeAsync(int courseId, int employeeId);
        
        Task<IEnumerable<CourseEnrollment>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<CourseEnrollment>> GetAllAsync();
        
        Task AddAsync(CourseEnrollment enrollment);
        Task AddRangeAsync(IEnumerable<CourseEnrollment> enrollments);
        Task UpdateAsync(CourseEnrollment enrollment);
        
        Task SaveChangesAsync();
    }
}
