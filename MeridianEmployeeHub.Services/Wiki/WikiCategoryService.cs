using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.Wiki.DTOs;

namespace MeridianEmployeeHub.Services.Wiki
{
    public class WikiCategoryService : IWikiCategoryService
    {
        private readonly IWikiCategoryRepository _repository;

        public WikiCategoryService(IWikiCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<WikiCategoryTreeDto>> GetCategoryTreeAsync()
        {
            var categories = await _repository.GetAllAsync();
            
            // Build tree in memory
            var lookup = categories.ToDictionary(c => c.Id, c => new WikiCategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Children = new List<WikiCategoryTreeDto>()
            });

            var rootNodes = new List<WikiCategoryTreeDto>();

            foreach (var category in categories)
            {
                var dto = lookup[category.Id];
                if (category.ParentCategoryId.HasValue && lookup.TryGetValue(category.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(dto);
                }
                else
                {
                    rootNodes.Add(dto);
                }
            }

            return rootNodes;
        }

        public async Task<WikiCategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(request.ParentCategoryId.Value)
                    ?? throw new KeyNotFoundException("Parent category not found.");
            }

            var baseSlug = SlugGenerator.GenerateSlug(request.Name);
            var finalSlug = await GenerateUniqueSlugAsync(baseSlug);

            var category = new WikiCategory
            {
                Name = request.Name,
                Slug = finalSlug,
                ParentCategoryId = request.ParentCategoryId,
                OrderIndex = request.OrderIndex
            };

            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<WikiCategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Category {id} not found.");

            if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == id)
                    throw new ConflictException("Category cannot be its own parent.");

                var parent = await _repository.GetByIdAsync(request.ParentCategoryId.Value)
                    ?? throw new KeyNotFoundException("Parent category not found.");
            }

            // Only update slug if name changed
            if (category.Name != request.Name)
            {
                category.Name = request.Name;
                var baseSlug = SlugGenerator.GenerateSlug(request.Name);
                category.Slug = await GenerateUniqueSlugAsync(baseSlug, id);
            }

            category.ParentCategoryId = request.ParentCategoryId;
            category.OrderIndex = request.OrderIndex;

            await _repository.UpdateAsync(category);
            await _repository.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Category {id} not found.");

            // EF Core will throw an exception if we try to delete a category that has children 
            // (because of Restrict delete behavior), which is what we want.
            try
            {
                await _repository.DeleteAsync(category);
                await _repository.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                throw new ConflictException("Cannot delete category because it contains subcategories or articles. Please reassign or delete them first.");
            }
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, int? excludeId = null)
        {
            var slug = baseSlug;
            int counter = 2;

            while (true)
            {
                var existing = await _repository.GetBySlugAsync(slug);
                if (existing == null || existing.Id == excludeId)
                {
                    return slug;
                }
                
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
        }

        private WikiCategoryDto MapToDto(WikiCategory category)
        {
            return new WikiCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentCategoryId = category.ParentCategoryId,
                OrderIndex = category.OrderIndex
            };
        }
    }
}
