using ChangelogSaas.Domain.Entities;

namespace ChangelogSaas.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
