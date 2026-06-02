using System.IdentityModel.Tokens.Jwt;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangelogSaas.Tests.Auth
{
    public class JwtTokenServiceTests
    {
        private static JwtTokenService CreateService() => new(Options.Create(new JwtOptions
        {
            Key = "test-key-test-key-test-key-1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        }));

        [Fact]
        public void GenerateToken_includes_sub_email_claims_and_expires_in_future()
        {
            var service = CreateService();
            var user = User.Create("alice@example.com", "irrelevant-hash", "Alice");

            var (token, expiresAt) = service.GenerateToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.True(expiresAt > DateTime.UtcNow);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Contains(jwt.Audiences, a => a == "TestAudience");
            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Equal("Alice", jwt.Claims.First(c => c.Type == "name").Value);
        }

        [Fact]
        public void Constructor_throws_when_key_too_short()
        {
            var bad = Options.Create(new JwtOptions { Key = "short", Issuer = "i", Audience = "a", ExpiryMinutes = 1 });
            Assert.Throws<InvalidOperationException>(() => new JwtTokenService(bad));
        }
    }
}
