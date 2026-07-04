using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class CourseEnrollmentRepository : ICourseEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseEnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseEnrollment?> GetByIdAsync(int id)
        {
            return await _context.CourseEnrollments
                .Include(ce => ce.Course)
                .Include(ce => ce.Employee)
                .FirstOrDefaultAsync(ce => ce.Id == id);
        }

        public async Task<CourseEnrollment?> GetByCourseAndEmployeeAsync(int courseId, int employeeId)
        {
            return await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.CourseId == courseId && ce.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<CourseEnrollment>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.CourseEnrollments
                .Include(ce => ce.Course)
                .Where(ce => ce.EmployeeId == employeeId)
                .OrderByDescending(ce => ce.EnrolledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseEnrollment>> GetAllAsync()
        {
            return await _context.CourseEnrollments
                .Include(ce => ce.Course)
                .Include(ce => ce.Employee)
                .OrderByDescending(ce => ce.EnrolledAt)
                .ToListAsync();
        }

        public async Task AddAsync(CourseEnrollment enrollment)
        {
            await _context.CourseEnrollments.AddAsync(enrollment);
        }

        public async Task AddRangeAsync(IEnumerable<CourseEnrollment> enrollments)
        {
            await _context.CourseEnrollments.AddRangeAsync(enrollments);
        }

        public async Task UpdateAsync(CourseEnrollment enrollment)
        {
            _context.CourseEnrollments.Update(enrollment);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
