using ChangelogSaas.Application.Common;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
    {
        private readonly IAppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public LoginCommandHandler(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens)
        {
            _db = db;
            _hasher = hasher;
            _tokens = tokens;
        }

        public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var normalized = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
            if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
                throw new ValidationException("Invalid email or password.");

            var (token, expiresAt) = _tokens.GenerateToken(user);
            return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
        }
    }
}
