using System.Security.Claims;
using ChangelogSaas.Application.Auth.Commands.Login;
using ChangelogSaas.Application.Auth.Commands.OAuthLogin;
using ChangelogSaas.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/auth");
            g.MapPost("/register", Register);
            g.MapPost("/login", Login);
            g.MapPost("/oauth", OAuthLogin);
            g.MapGet("/me", Me).RequireAuthorization();
            return app;
        }

        private static async Task<IResult> Register([FromBody] RegisterCommand command, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(command, ct));

        private static async Task<IResult> Login([FromBody] LoginCommand command, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(command, ct));

        private static async Task<IResult> OAuthLogin([FromBody] OAuthLoginCommand command, ISender sender, CancellationToken ct)
            => Results.Ok(await sender.Send(command, ct));

        private static IResult Me(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue("email") ?? user.FindFirstValue(ClaimTypes.Email);
            var name = user.FindFirstValue("name");
            return Results.Ok(new { userId, email, displayName = name });
        }
    }
}
