using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class TrainingCourseRepository : ITrainingCourseRepository
    {
        private readonly ApplicationDbContext _context;

        public TrainingCourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingCourse?> GetByIdAsync(int id)
        {
            return await _context.TrainingCourses.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<TrainingCourse?> GetByIdWithModulesAsync(int id)
        {
            return await _context.TrainingCourses
                .Include(c => c.Modules.OrderBy(m => m.OrderIndex))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<IEnumerable<TrainingCourse>> GetAllAsync(TrainingCategory? category = null, string? search = null)
        {
            var query = _context.TrainingCourses.Where(c => c.IsActive);

            if (category.HasValue)
            {
                query = query.Where(c => c.Category == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<TrainingCourse>> GetMandatoryCoursesAsync()
        {
            return await _context.TrainingCourses
                .Where(c => c.IsActive && c.IsMandatoryForNewHires)
                .ToListAsync();
        }

        public async Task AddAsync(TrainingCourse course)
        {
            await _context.TrainingCourses.AddAsync(course);
        }

        public async Task UpdateAsync(TrainingCourse course)
        {
            _context.TrainingCourses.Update(course);
            await Task.CompletedTask;
        }

        public async Task AddModuleAsync(TrainingModule module)
        {
            await _context.TrainingModules.AddAsync(module);
        }

        public async Task<TrainingModule?> GetModuleByIdAsync(int courseId, int moduleId)
        {
            return await _context.TrainingModules
                .FirstOrDefaultAsync(m => m.CourseId == courseId && m.Id == moduleId);
        }

        public async Task UpdateModuleAsync(TrainingModule module)
        {
            _context.TrainingModules.Update(module);
            await Task.CompletedTask;
        }

        public async Task DeleteModuleAsync(TrainingModule module)
        {
            _context.TrainingModules.Remove(module);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
