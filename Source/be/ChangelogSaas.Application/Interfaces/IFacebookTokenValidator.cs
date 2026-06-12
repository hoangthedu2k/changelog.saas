namespace ChangelogSaas.Application.Interfaces
{
    public interface IFacebookTokenValidator
    {
        Task<OAuthUserInfo> ValidateAsync(string accessToken);
    }
}
