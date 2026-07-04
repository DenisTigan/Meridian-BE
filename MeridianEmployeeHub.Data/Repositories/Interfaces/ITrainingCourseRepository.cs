using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface ITrainingCourseRepository
    {
        Task<TrainingCourse?> GetByIdAsync(int id);
        Task<TrainingCourse?> GetByIdWithModulesAsync(int id);
        Task<IEnumerable<TrainingCourse>> GetAllAsync(TrainingCategory? category = null, string? search = null);
        Task<IEnumerable<TrainingCourse>> GetMandatoryCoursesAsync();
        
        Task AddAsync(TrainingCourse course);
        Task UpdateAsync(TrainingCourse course);

        Task AddModuleAsync(TrainingModule module);
        Task<TrainingModule?> GetModuleByIdAsync(int courseId, int moduleId);
        Task UpdateModuleAsync(TrainingModule module);
        Task DeleteModuleAsync(TrainingModule module);
        
        Task SaveChangesAsync();
    }
}
