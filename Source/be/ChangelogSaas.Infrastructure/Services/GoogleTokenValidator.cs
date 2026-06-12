using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace ChangelogSaas.Infrastructure.Services
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly string _clientId;

        public GoogleTokenValidator(IConfiguration configuration)
        {
            _clientId = configuration["OAuth:Google:ClientId"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientId is missing.");
        }

        public async Task<OAuthUserInfo> ValidateAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_clientId]
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return new OAuthUserInfo(payload.Email, payload.Name ?? payload.Email, payload.Subject);
            }
            catch (InvalidJwtException ex)
            {
                throw new ValidationException($"Invalid Google token: {ex.Message}");
            }
        }
    }
}
