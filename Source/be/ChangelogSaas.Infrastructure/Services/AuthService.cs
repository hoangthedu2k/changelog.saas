using ChangelogSaas.Application.Common;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using ChangelogSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public AuthService(AppDbContext db, IPasswordHasher hasher, ITokenService tokens)
        {
            _db = db;
            _hasher = hasher;
            _tokens = tokens;
        }

        public async Task<AuthResult> RegisterAsync(string email, string password, string? displayName, CancellationToken cancellationToken = default)
        {
            var normalized = email.Trim().ToLowerInvariant();

            var exists = await _db.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
            if (exists)
                throw new ValidationException("Email already exists.");

            var user = User.Create(normalized, _hasher.Hash(password), displayName);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return BuildResult(user);
        }

        public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var normalized = email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
            if (user is null || !_hasher.Verify(password, user.PasswordHash))
                throw new ValidationException("Invalid email or password.");

            return BuildResult(user);
        }

        private AuthResult BuildResult(User user)
        {
            var (token, expiresAt) = _tokens.GenerateToken(user);
            return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
        }
    }
}
