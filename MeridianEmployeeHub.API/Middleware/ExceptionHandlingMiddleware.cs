using MeridianEmployeeHub.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MeridianEmployeeHub.API.Middleware
{
    // Middleware global de tratare a exceptiilor.
    // Produce raspunsuri in format RFC 7807 (Problem Details) pentru toate erorile necaptate.
    // Trebuie inregistrat primul in pipeline: app.UseMiddleware<ExceptionHandlingMiddleware>()
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = exception switch
            {
                ForbiddenException forbiddenEx =>
                    (StatusCodes.Status403Forbidden, "Forbidden", forbiddenEx.Message),

                UnauthorizedAccessException unauthorizedEx =>
                    (StatusCodes.Status401Unauthorized, "Unauthorized", unauthorizedEx.Message),

                KeyNotFoundException notFoundEx =>
                    (StatusCodes.Status404NotFound, "Not Found", notFoundEx.Message),

                ConflictException conflictEx =>
                    (StatusCodes.Status409Conflict, "Conflict", conflictEx.Message),

                ArgumentException argEx =>
                    (StatusCodes.Status400BadRequest, "Bad Request", argEx.Message),

                InvalidOperationException invalidOpEx =>
                    (StatusCodes.Status400BadRequest, "Invalid Operation", invalidOpEx.Message),

                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error",
                      "An unexpected error occurred. Please try again later.")
            };

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
        }
    }
}
