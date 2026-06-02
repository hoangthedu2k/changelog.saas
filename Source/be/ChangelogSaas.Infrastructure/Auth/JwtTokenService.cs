using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChangelogSaas.Infrastructure.Auth
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            if (string.IsNullOrWhiteSpace(_options.Key) || _options.Key.Length < 32)
                throw new InvalidOperationException("Jwt:Key must be set and at least 32 characters long.");
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(_options.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (!string.IsNullOrEmpty(user.DisplayName))
                claims.Add(new Claim("name", user.DisplayName));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
