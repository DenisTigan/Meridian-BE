using System.Security.Claims;
using MeridianEmployeeHub.Services.Wiki;
using MeridianEmployeeHub.Services.Wiki.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/wiki/articles")]
    public class WikiArticlesController : ControllerBase
    {
        private readonly IWikiArticleService _articleService;

        public WikiArticlesController(IWikiArticleService articleService)
        {
            _articleService = articleService;
        }

        // GET /api/v1/wiki/articles
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WikiArticleDto>>> GetArticles(
            [FromQuery] int? categoryId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var isHROrAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
            var currentUserId = GetCurrentEmployeeId();

            var articles = await _articleService.GetArticlesAsync(categoryId, null, skip, take, isHROrAdmin, currentUserId);
            return Ok(articles);
        }

        // GET /api/v1/wiki/search
        // This is mapped to /api/v1/wiki/search, so the route on the class is /api/v1/wiki/articles. Wait. 
        // Plan says: GET /api/v1/wiki/search
        // So I'll put it here but with Route("/api/v1/wiki/search")
        [HttpGet("/api/v1/wiki/search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WikiArticleDto>>> SearchArticles(
            [FromQuery] string q,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var isHROrAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
            var currentUserId = GetCurrentEmployeeId();

            var articles = await _articleService.GetArticlesAsync(null, q, skip, take, isHROrAdmin, currentUserId);
            return Ok(articles);
        }

        // GET /api/v1/wiki/articles/{slug}
        [HttpGet("{slug}")]
        [Authorize]
        public async Task<ActionResult<WikiArticleDto>> GetArticleBySlug(string slug)
        {
            var isHROrAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
            var currentUserId = GetCurrentEmployeeId();

            var article = await _articleService.GetArticleBySlugAsync(slug, isHROrAdmin, currentUserId);
            return Ok(article);
        }

        // POST /api/v1/wiki/articles
        [HttpPost]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<ActionResult<WikiArticleDto>> CreateArticle([FromBody] CreateArticleRequest request)
        {
            var currentUserId = GetCurrentEmployeeId();
            var article = await _articleService.CreateArticleAsync(request, currentUserId);
            return StatusCode(StatusCodes.Status201Created, article);
        }

        // PUT /api/v1/wiki/articles/{slug}
        [HttpPut("{slug}")]
        [Authorize]
        public async Task<ActionResult<WikiArticleDto>> UpdateArticle(string slug, [FromBody] UpdateArticleRequest request)
        {
            var isHROrAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
            var currentUserId = GetCurrentEmployeeId();

            var article = await _articleService.UpdateArticleAsync(slug, request, isHROrAdmin, currentUserId);
            return Ok(article);
        }

        // DELETE /api/v1/wiki/articles/{slug}
        [HttpDelete("{slug}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteArticle(string slug)
        {
            await _articleService.DeleteArticleAsync(slug);
            return NoContent();
        }

        private int GetCurrentEmployeeId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException("Invalid token: missing or invalid subject claim.");

            return employeeId;
        }
    }
}
