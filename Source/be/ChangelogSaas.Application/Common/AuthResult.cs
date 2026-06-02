namespace ChangelogSaas.Application.Common
{
    public sealed record AuthResult(
        string Token,
        DateTime ExpiresAt,
        Guid UserId,
        string Email,
        string? DisplayName);
}
