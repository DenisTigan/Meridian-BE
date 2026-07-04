using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.Training.DTOs;

namespace MeridianEmployeeHub.Services.Training
{
    public class CourseEnrollmentService : ICourseEnrollmentService
    {
        private readonly ICourseEnrollmentRepository _repository;
        private readonly ITrainingCourseRepository _courseRepository;

        public CourseEnrollmentService(
            ICourseEnrollmentRepository repository,
            ITrainingCourseRepository courseRepository)
        {
            _repository = repository;
            _courseRepository = courseRepository;
        }

        public async Task<CourseEnrollmentDto> EnrollEmployeeAsync(int courseId, int employeeId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId)
                ?? throw new KeyNotFoundException($"Course {courseId} not found.");

            var existing = await _repository.GetByCourseAndEmployeeAsync(courseId, employeeId);
            if (existing != null)
            {
                throw new ConflictException("Already enrolled in this course.");
            }

            var enrollment = new CourseEnrollment
            {
                CourseId = courseId,
                EmployeeId = employeeId,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercent = 0,
                IsCompleted = false
            };

            await _repository.AddAsync(enrollment);
            await _repository.SaveChangesAsync();

            // Need to reload to get navigational properties like CourseTitle, EmployeeName if needed by DTO
            var reloaded = await _repository.GetByIdAsync(enrollment.Id);
            return MapToDto(reloaded ?? enrollment);
        }

        public async Task<CourseEnrollmentDto> UpdateProgressAsync(int enrollmentId, int employeeId, byte progressPercent)
        {
            var enrollment = await _repository.GetByIdAsync(enrollmentId)
                ?? throw new KeyNotFoundException($"Enrollment {enrollmentId} not found.");

            if (enrollment.EmployeeId != employeeId)
            {
                throw new ForbiddenException("You can only update your own progress.");
            }

            if (progressPercent > 100)
            {
                throw new ArgumentException("Progress percent cannot be greater than 100.");
            }

            enrollment.ProgressPercent = progressPercent;
            enrollment.LastAccessedAt = DateTime.UtcNow;

            if (progressPercent == 100 && !enrollment.IsCompleted)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletedAt = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(enrollment);
            await _repository.SaveChangesAsync();

            return MapToDto(enrollment);
        }

        public async Task<IEnumerable<CourseEnrollmentDto>> GetMyEnrollmentsAsync(int employeeId)
        {
            var enrollments = await _repository.GetByEmployeeIdAsync(employeeId);
            return enrollments.Select(MapToDto);
        }

        public async Task<IEnumerable<CourseEnrollmentDto>> GetAllEnrollmentsAsync()
        {
            var enrollments = await _repository.GetAllAsync();
            return enrollments.Select(MapToDto);
        }

        public async Task<object> GetCertificateStatusAsync(int enrollmentId, int employeeId)
        {
            var enrollment = await _repository.GetByIdAsync(enrollmentId)
                ?? throw new KeyNotFoundException($"Enrollment {enrollmentId} not found.");

            if (enrollment.EmployeeId != employeeId)
            {
                throw new ForbiddenException("You can only access your own certificate.");
            }

            if (!enrollment.IsCompleted)
            {
                throw new ConflictException("Course not yet completed.");
            }

            // Always return null for CertificateUrl in MVP
            return new
            {
                certificateUrl = (string?)null,
                message = "Certificate generation not yet available"
            };
        }

        private CourseEnrollmentDto MapToDto(CourseEnrollment enrollment)
        {
            return new CourseEnrollmentDto
            {
                Id = enrollment.Id,
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.Course?.Title ?? string.Empty,
                EmployeeId = enrollment.EmployeeId,
                EmployeeName = enrollment.Employee != null ? $"{enrollment.Employee.FirstName} {enrollment.Employee.LastName}" : string.Empty,
                EnrolledAt = enrollment.EnrolledAt,
                LastAccessedAt = enrollment.LastAccessedAt,
                CompletedAt = enrollment.CompletedAt,
                ProgressPercent = enrollment.ProgressPercent,
                IsCompleted = enrollment.IsCompleted,
                CertificateUrl = enrollment.CertificateUrl
            };
        }
    }
}
