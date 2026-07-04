using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.Training.DTOs;

namespace MeridianEmployeeHub.Services.Training
{
    public class TrainingCourseService : ITrainingCourseService
    {
        private readonly ITrainingCourseRepository _repository;

        public TrainingCourseService(ITrainingCourseRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TrainingCourseDto>> GetCoursesAsync(TrainingCategory? category = null, string? search = null)
        {
            var courses = await _repository.GetAllAsync(category, search);
            return courses.Select(MapToDto);
        }

        public async Task<TrainingCourseDto> GetCourseByIdAsync(int id)
        {
            var course = await _repository.GetByIdWithModulesAsync(id)
                ?? throw new KeyNotFoundException($"Course {id} not found.");

            return MapToDto(course);
        }

        public async Task<TrainingCourseDto> CreateCourseAsync(CreateCourseRequest request, int currentUserId)
        {
            var course = new TrainingCourse
            {
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                EstimatedMinutes = request.EstimatedMinutes,
                ThumbnailUrl = request.ThumbnailUrl,
                IsMandatoryForNewHires = request.IsMandatoryForNewHires,
                IsActive = true,
                CreatedById = currentUserId
            };

            await _repository.AddAsync(course);
            await _repository.SaveChangesAsync();

            return MapToDto(course);
        }

        public async Task<TrainingCourseDto> UpdateCourseAsync(int id, UpdateCourseRequest request)
        {
            var course = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Course {id} not found.");

            course.Title = request.Title;
            course.Description = request.Description;
            course.Category = request.Category;
            course.EstimatedMinutes = request.EstimatedMinutes;
            course.ThumbnailUrl = request.ThumbnailUrl;
            course.IsMandatoryForNewHires = request.IsMandatoryForNewHires;
            course.IsActive = request.IsActive;

            await _repository.UpdateAsync(course);
            await _repository.SaveChangesAsync();

            return MapToDto(course);
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Course {id} not found.");

            course.IsActive = false;
            await _repository.UpdateAsync(course);
            await _repository.SaveChangesAsync();
        }

        public async Task<TrainingModuleDto> AddModuleAsync(int courseId, CreateModuleRequest request)
        {
            var course = await _repository.GetByIdAsync(courseId)
                ?? throw new KeyNotFoundException($"Course {courseId} not found.");

            var module = new TrainingModule
            {
                CourseId = course.Id,
                Title = request.Title,
                Content = request.Content,
                ModuleType = request.ModuleType,
                OrderIndex = request.OrderIndex,
                DurationMinutes = request.DurationMinutes
            };

            await _repository.AddModuleAsync(module);
            await _repository.SaveChangesAsync();

            return MapModuleToDto(module);
        }

        public async Task<TrainingModuleDto> UpdateModuleAsync(int courseId, int moduleId, CreateModuleRequest request)
        {
            var module = await _repository.GetModuleByIdAsync(courseId, moduleId)
                ?? throw new KeyNotFoundException($"Module {moduleId} not found in course {courseId}.");

            module.Title = request.Title;
            module.Content = request.Content;
            module.ModuleType = request.ModuleType;
            module.OrderIndex = request.OrderIndex;
            module.DurationMinutes = request.DurationMinutes;

            await _repository.UpdateModuleAsync(module);
            await _repository.SaveChangesAsync();

            return MapModuleToDto(module);
        }

        public async Task DeleteModuleAsync(int courseId, int moduleId)
        {
            var module = await _repository.GetModuleByIdAsync(courseId, moduleId)
                ?? throw new KeyNotFoundException($"Module {moduleId} not found in course {courseId}.");

            await _repository.DeleteModuleAsync(module);
            await _repository.SaveChangesAsync();
        }

        private TrainingCourseDto MapToDto(TrainingCourse course)
        {
            return new TrainingCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Category = course.Category,
                EstimatedMinutes = course.EstimatedMinutes,
                ThumbnailUrl = course.ThumbnailUrl,
                IsMandatoryForNewHires = course.IsMandatoryForNewHires,
                CreatedById = course.CreatedById,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                Modules = course.Modules?.Select(MapModuleToDto).ToList() ?? new List<TrainingModuleDto>()
            };
        }

        private TrainingModuleDto MapModuleToDto(TrainingModule module)
        {
            return new TrainingModuleDto
            {
                Id = module.Id,
                CourseId = module.CourseId,
                Title = module.Title,
                Content = module.Content,
                ModuleType = module.ModuleType,
                OrderIndex = module.OrderIndex,
                DurationMinutes = module.DurationMinutes
            };
        }
    }
}
