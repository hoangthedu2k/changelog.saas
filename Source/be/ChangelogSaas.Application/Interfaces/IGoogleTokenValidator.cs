namespace ChangelogSaas.Application.Interfaces
{
    public record OAuthUserInfo(string Email, string Name, string ProviderId);

    public interface IGoogleTokenValidator
    {
        Task<OAuthUserInfo> ValidateAsync(string idToken);
    }
}
