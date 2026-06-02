using ChangelogSaas.Application.Common;

namespace ChangelogSaas.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string email, string password, string? displayName, CancellationToken cancellationToken = default);
        Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    }
}
