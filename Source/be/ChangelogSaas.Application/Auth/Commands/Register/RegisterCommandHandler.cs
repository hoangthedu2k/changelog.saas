using ChangelogSaas.Application.Common;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
    {
        private readonly IAppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;

        public RegisterCommandHandler(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens)
        {
            _db = db;
            _hasher = hasher;
            _tokens = tokens;
        }

        public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var normalized = request.Email.Trim().ToLowerInvariant();

            var exists = await _db.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
            if (exists)
                throw new ValidationException("Email already exists.");

            var user = User.Create(normalized, _hasher.Hash(request.Password), request.DisplayName);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            var (token, expiresAt) = _tokens.GenerateToken(user);
            return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
        }
    }
}
