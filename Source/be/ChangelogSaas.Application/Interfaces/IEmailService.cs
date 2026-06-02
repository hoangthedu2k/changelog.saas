namespace ChangelogSaas.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string confirmUrl);
        Task SendChangelogNotificationAsync(string toEmail, string projectName, string entryTitle, string entryUrl, string unsubscribeUrl);
    }
}
