using System.Text.Json;
using ChangelogSaas.Domain.Exceptions;

namespace ChangelogSaas.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            catch (ValidationException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Validation failed", ex.Message);
            }
            catch (NotFoundException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
            }
            catch (DomainException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Domain rule violated", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.");
            }
        }

        private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
        {
            if (context.Response.HasStarted) return Task.CompletedTask;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var payload = JsonSerializer.Serialize(new
            {
                type = $"https://httpstatuses.io/{statusCode}",
                title,
                status = statusCode,
                detail
            });
            return context.Response.WriteAsync(payload);
        }
    }
}
