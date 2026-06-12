using System.Text.Json;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Exceptions;

namespace ChangelogSaas.Infrastructure.Services
{
    public class FacebookTokenValidator : IFacebookTokenValidator
    {
        private readonly HttpClient _http;

        public FacebookTokenValidator(HttpClient http)
        {
            _http = http;
        }

        public async Task<OAuthUserInfo> ValidateAsync(string accessToken)
        {
            var url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";
            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ValidationException("Invalid Facebook token.");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out _))
                throw new ValidationException("Invalid Facebook token.");

            var id = root.GetProperty("id").GetString() ?? throw new ValidationException("Facebook id missing.");
            var name = root.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
            var email = root.TryGetProperty("email", out var e) ? e.GetString() : null;

            if (string.IsNullOrEmpty(email))
                throw new ValidationException("Facebook account must have an email address. Please check your Facebook privacy settings.");

            return new OAuthUserInfo(email, name, id);
        }
    }
}
