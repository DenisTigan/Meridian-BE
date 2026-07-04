using MeridianEmployeeHub.Services.Wiki;
using MeridianEmployeeHub.Services.Wiki.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/wiki/categories")]
    public class WikiCategoriesController : ControllerBase
    {
        private readonly IWikiCategoryService _categoryService;

        public WikiCategoriesController(IWikiCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET /api/v1/wiki/categories
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WikiCategoryTreeDto>>> GetCategories()
        {
            var tree = await _categoryService.GetCategoryTreeAsync();
            return Ok(tree);
        }

        // POST /api/v1/wiki/categories
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<WikiCategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            var category = await _categoryService.CreateCategoryAsync(request);
            return StatusCode(StatusCodes.Status201Created, category);
        }

        // PUT /api/v1/wiki/categories/{id}
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<WikiCategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _categoryService.UpdateCategoryAsync(id, request);
            return Ok(category);
        }

        // DELETE /api/v1/wiki/categories/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
