namespace ChangelogSaas.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string confirmUrl);
        Task SendChangelogNotificationAsync(
            string toEmail,
            string projectName,
            string entryTitle,
            string entryContentHtml,
            List<string> entryTags,
            string? entryVersion,
            DateTime? entryPublishedAt,
            string entryUrl,
            string unsubscribeUrl);
    }
}
