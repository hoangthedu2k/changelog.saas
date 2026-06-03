using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChangelogSaas.Domain.Exceptions;

namespace ChangelogSaas.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var raw = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId))
                throw new ValidationException("Invalid or missing user claim.");

            return userId;
        }
    }
}
