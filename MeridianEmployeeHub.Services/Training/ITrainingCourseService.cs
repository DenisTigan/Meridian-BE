using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Training.DTOs;

namespace MeridianEmployeeHub.Services.Training
{
    public interface ITrainingCourseService
    {
        Task<IEnumerable<TrainingCourseDto>> GetCoursesAsync(TrainingCategory? category = null, string? search = null);
        Task<TrainingCourseDto> GetCourseByIdAsync(int id);
        
        Task<TrainingCourseDto> CreateCourseAsync(CreateCourseRequest request, int currentUserId);
        Task<TrainingCourseDto> UpdateCourseAsync(int id, UpdateCourseRequest request);
        Task DeleteCourseAsync(int id);

        Task<TrainingModuleDto> AddModuleAsync(int courseId, CreateModuleRequest request);
        Task<TrainingModuleDto> UpdateModuleAsync(int courseId, int moduleId, CreateModuleRequest request);
        Task DeleteModuleAsync(int courseId, int moduleId);
    }
}
