using ChangelogSaas.Application.Common;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Auth.Commands.OAuthLogin
{
    public class OAuthLoginCommandHandler : IRequestHandler<OAuthLoginCommand, AuthResult>
    {
        private readonly IAppDbContext _db;
        private readonly ITokenService _tokens;
        private readonly IGoogleTokenValidator _google;
        private readonly IFacebookTokenValidator _facebook;

        public OAuthLoginCommandHandler(
            IAppDbContext db,
            ITokenService tokens,
            IGoogleTokenValidator google,
            IFacebookTokenValidator facebook)
        {
            _db = db;
            _tokens = tokens;
            _google = google;
            _facebook = facebook;
        }

        public async Task<AuthResult> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
        {
            var provider = request.Provider.ToLowerInvariant();

            OAuthUserInfo info = provider switch
            {
                "google" => await _google.ValidateAsync(request.Token),
                "facebook" => await _facebook.ValidateAsync(request.Token),
                _ => throw new ValidationException($"Unsupported OAuth provider: {request.Provider}")
            };

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == info.Email, cancellationToken);

            if (user is null)
            {
                user = User.CreateFromOAuth(info.Email, info.Name, provider, info.ProviderId);
                _db.Users.Add(user);
            }
            else if (user.OAuthProviderId is null)
            {
                user.LinkOAuth(provider, info.ProviderId);
            }

            await _db.SaveChangesAsync(cancellationToken);

            var (token, expiresAt) = _tokens.GenerateToken(user);
            return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
        }
    }
}
