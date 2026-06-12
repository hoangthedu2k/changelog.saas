using ChangelogSaas.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Resend;

namespace ChangelogSaas.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly string _fromAddress;
        private readonly string _fromName;

        public EmailService(IResend resend, IConfiguration config)
        {
            _resend = resend;
            _fromAddress = config["Resend:FromAddress"]
                ?? throw new InvalidOperationException("Resend:FromAddress is missing.");
            _fromName = config["Resend:FromName"] ?? "Changelog";
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string confirmUrl)
        {
            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromAddress}>",
                To = { toEmail },
                Subject = "Confirm your subscription",
                HtmlBody = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px 0">
                      <h2 style="margin:0 0 8px">Confirm your subscription</h2>
                      <p style="color:#555;margin:0 0 24px">Click the button below to confirm your email address and start receiving changelog updates.</p>
                      <a href="{confirmUrl}"
                         style="display:inline-block;padding:12px 24px;background:#6366f1;color:#fff;border-radius:6px;text-decoration:none;font-weight:500">
                        Confirm subscription
                      </a>
                      <p style="color:#999;font-size:12px;margin:24px 0 0">
                        If you did not subscribe, you can safely ignore this email.
                      </p>
                    </div>
                    """,
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendChangelogNotificationAsync(
            string toEmail, string projectName, string entryTitle, string entryUrl, string unsubscribeUrl)
        {
            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromAddress}>",
                To = { toEmail },
                Subject = $"[{projectName}] {entryTitle}",
                HtmlBody = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px 0">
                      <p style="color:#888;font-size:12px;margin:0 0 4px;text-transform:uppercase;letter-spacing:.5px">{projectName}</p>
                      <h2 style="margin:0 0 16px">{entryTitle}</h2>
                      <a href="{entryUrl}"
                         style="display:inline-block;padding:12px 24px;background:#6366f1;color:#fff;border-radius:6px;text-decoration:none;font-weight:500">
                        Read update
                      </a>
                      <p style="color:#999;font-size:12px;margin:32px 0 0">
                        You are receiving this because you subscribed to {projectName} updates.
                        <a href="{unsubscribeUrl}" style="color:#999">Unsubscribe</a>
                      </p>
                    </div>
                    """,
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
