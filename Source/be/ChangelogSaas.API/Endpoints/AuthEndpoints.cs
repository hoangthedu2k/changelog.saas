using System.Net.Mail;
using System.Security.Claims;
using ChangelogSaas.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class AuthEndpoints
    {
        public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
        public sealed record LoginRequest(string Email, string Password);

        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/auth");
            g.MapPost("/register", Register);
            g.MapPost("/login", Login);
            g.MapGet("/me", Me).RequireAuthorization();
            return app;
        }

        private static async Task<IResult> Register([FromBody] RegisterRequest req, IAuthService auth, CancellationToken ct)
        {
            var error = ValidateCredentials(req.Email, req.Password);
            if (error is not null) return Results.BadRequest(new { error });

            var result = await auth.RegisterAsync(req.Email, req.Password, req.DisplayName, ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> Login([FromBody] LoginRequest req, IAuthService auth, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest(new { error = "Email and password are required." });

            var result = await auth.LoginAsync(req.Email, req.Password, ct);
            return Results.Ok(result);
        }

        private static IResult Me(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
            var name = user.FindFirstValue("name");
            return Results.Ok(new { userId, email, displayName = name });
        }

        private static string? ValidateCredentials(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Email is required.";
            if (!MailAddress.TryCreate(email, out _)) return "Email format is invalid.";
            if (string.IsNullOrEmpty(password)) return "Password is required.";
            if (password.Length < 8) return "Password must be at least 8 characters.";
            return null;
        }
    }
}
