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
                    <!DOCTYPE html>
                    <html lang="en">
                    <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
                    <body style="margin:0;padding:0;background:#f4f4f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif">
                      <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f5;padding:40px 16px">
                        <tr><td align="center">
                          <table width="100%" style="max-width:520px" cellpadding="0" cellspacing="0">

                            <!-- Header -->
                            <tr><td style="background:#6366f1;border-radius:8px 8px 0 0;padding:4px 0 0;line-height:0">&nbsp;</td></tr>

                            <!-- Card -->
                            <tr><td style="background:#ffffff;border-radius:0 0 8px 8px;padding:40px 40px 32px;box-shadow:0 1px 3px rgba(0,0,0,.08)">

                              <!-- Brand -->
                              <p style="margin:0 0 32px;font-size:18px;font-weight:700;color:#111827;letter-spacing:-.3px">{_fromName}</p>

                              <!-- Title -->
                              <h1 style="margin:0 0 12px;font-size:22px;font-weight:700;color:#111827;line-height:1.3">Confirm your subscription</h1>
                              <p style="margin:0 0 32px;font-size:15px;color:#6b7280;line-height:1.6">
                                You're one step away. Click the button below to confirm your email address and start receiving product updates directly in your inbox.
                              </p>

                              <!-- CTA -->
                              <table cellpadding="0" cellspacing="0"><tr><td>
                                <a href="{confirmUrl}"
                                   style="display:inline-block;padding:13px 28px;background:#6366f1;color:#ffffff;border-radius:6px;text-decoration:none;font-size:15px;font-weight:600;letter-spacing:-.1px">
                                  Confirm subscription &rarr;
                                </a>
                              </td></tr></table>

                              <!-- Fallback URL -->
                              <p style="margin:24px 0 0;font-size:12px;color:#9ca3af;line-height:1.5">
                                Button not working? Copy and paste this link into your browser:<br>
                                <a href="{confirmUrl}" style="color:#6366f1;word-break:break-all">{confirmUrl}</a>
                              </p>

                              <!-- Divider -->
                              <hr style="border:none;border-top:1px solid #f3f4f6;margin:28px 0">

                              <!-- Footer note -->
                              <p style="margin:0;font-size:12px;color:#9ca3af;line-height:1.5">
                                If you did not request this, you can safely ignore this email. This link will expire in 24 hours.
                              </p>
                            </td></tr>

                            <!-- Footer -->
                            <tr><td style="padding:20px 0;text-align:center">
                              <p style="margin:0;font-size:12px;color:#9ca3af">&copy; {DateTime.UtcNow.Year} {_fromName}. All rights reserved.</p>
                            </td></tr>

                          </table>
                        </td></tr>
                      </table>
                    </body>
                    </html>
                    """,
            };

            await _resend.EmailSendAsync(message);
        }

        public async Task SendChangelogNotificationAsync(
            string toEmail,
            string projectName,
            string entryTitle,
            string entryContentHtml,
            List<string> entryTags,
            string? entryVersion,
            DateTime? entryPublishedAt,
            string entryUrl,
            string unsubscribeUrl)
        {
            var versionBadge = entryVersion is not null
                ? $"<span style=\"display:inline-block;padding:2px 8px;background:#f3f4f6;color:#374151;border-radius:4px;font-size:11px;font-weight:600;font-family:monospace;margin-left:8px\">{entryVersion}</span>"
                : "";

            var tagPills = entryTags.Count > 0
                ? string.Concat(entryTags.Select(t =>
                    $"<span style=\"display:inline-block;padding:3px 10px;background:#f3f4f6;color:#374151;border-radius:999px;font-size:11px;font-weight:500;margin:0 4px 4px 0\">{t}</span>"))
                : "";

            var publishedDate = entryPublishedAt.HasValue
                ? $"<p style=\"margin:0 0 20px;font-size:12px;color:#9ca3af\">{entryPublishedAt.Value:MMMM d, yyyy}</p>"
                : "";

            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromAddress}>",
                To = { toEmail },
                Subject = $"[{projectName}] {entryTitle}",
                HtmlBody = $"""
                    <!DOCTYPE html>
                    <html lang="en">
                    <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
                    <body style="margin:0;padding:0;background:#f4f4f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif">
                      <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f4f5;padding:40px 16px">
                        <tr><td align="center">
                          <table width="100%" style="max-width:560px" cellpadding="0" cellspacing="0">

                            <!-- Header accent -->
                            <tr><td style="background:#6366f1;border-radius:8px 8px 0 0;padding:4px 0 0;line-height:0">&nbsp;</td></tr>

                            <!-- Card -->
                            <tr><td style="background:#ffffff;border-radius:0 0 8px 8px;padding:40px 40px 32px;box-shadow:0 1px 3px rgba(0,0,0,.08)">

                              <!-- Brand + project -->
                              <p style="margin:0 0 4px;font-size:12px;font-weight:600;color:#6366f1;text-transform:uppercase;letter-spacing:.8px">{projectName}</p>
                              <p style="margin:0 0 28px;font-size:18px;font-weight:700;color:#111827;letter-spacing:-.3px">{_fromName}</p>
                              <hr style="border:none;border-top:1px solid #f3f4f6;margin:0 0 28px">

                              <!-- "New update" badge + version -->
                              <p style="margin:0 0 14px">
                                <span style="display:inline-block;padding:3px 10px;background:#ede9fe;color:#6366f1;border-radius:999px;font-size:11px;font-weight:600;letter-spacing:.4px;text-transform:uppercase">New update</span>
                                {versionBadge}
                              </p>

                              <!-- Title -->
                              <h1 style="margin:0 0 6px;font-size:22px;font-weight:700;color:#111827;line-height:1.35">{entryTitle}</h1>

                              <!-- Published date -->
                              {publishedDate}

                              <!-- Tags -->
                              {(tagPills.Length > 0 ? $"<p style=\"margin:0 0 24px\">{tagPills}</p>" : "")}

                              <!-- Entry content -->
                              <div style="font-size:15px;color:#374151;line-height:1.7;margin:0 0 28px">
                                {entryContentHtml}
                              </div>

                              <hr style="border:none;border-top:1px solid #f3f4f6;margin:0 0 24px">

                              <!-- CTA -->
                              <table cellpadding="0" cellspacing="0"><tr><td>
                                <a href="{entryUrl}"
                                   style="display:inline-block;padding:13px 28px;background:#6366f1;color:#ffffff;border-radius:6px;text-decoration:none;font-size:15px;font-weight:600;letter-spacing:-.1px">
                                  Read more &rarr;
                                </a>
                              </td></tr></table>

                              <!-- Divider -->
                              <hr style="border:none;border-top:1px solid #f3f4f6;margin:28px 0 20px">

                              <!-- Footer note -->
                              <p style="margin:0;font-size:12px;color:#9ca3af;line-height:1.6">
                                You're receiving this because you subscribed to updates from <strong style="color:#6b7280">{projectName}</strong>.
                                Don't want these emails? <a href="{unsubscribeUrl}" style="color:#6366f1;text-decoration:none">Unsubscribe</a>
                              </p>
                            </td></tr>

                            <!-- Footer -->
                            <tr><td style="padding:20px 0;text-align:center">
                              <p style="margin:0;font-size:12px;color:#9ca3af">&copy; {DateTime.UtcNow.Year} {_fromName}. All rights reserved.</p>
                            </td></tr>

                          </table>
                        </td></tr>
                      </table>
                    </body>
                    </html>
                    """,
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
