using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ChangelogSaas.Infrastructure.Jobs
{
    public class NotifySubscribersJob
    {
        private readonly IAppDbContext _db;
        private readonly IEmailService _email;
        private readonly string _publicUrl;

        public NotifySubscribersJob(IAppDbContext db, IEmailService email, IConfiguration config)
        {
            _db = db;
            _email = email;
            _publicUrl = config["App:PublicUrl"] ?? "http://localhost:4200";
        }

        public async Task ExecuteAsync(Guid entryId)
        {
            var entry = await _db.ChangelogEntries.FindAsync(entryId);
            if (entry is null) return;

            var project = await _db.Projects.FindAsync(entry.ProjectId);
            if (project is null) return;

            var subscribers = await _db.Subscribers
                .Where(s => s.ProjectId == entry.ProjectId && s.Status == SubscriberStatus.Verified)
                .ToListAsync();

            var entryUrl = $"{_publicUrl}/c/{project.Slug}";

            var tasks = subscribers.Select(s =>
            {
                var unsubscribeUrl = $"{_publicUrl}/unsubscribe?token={s.UnsubscribeToken}";
                return _email.SendChangelogNotificationAsync(
                    s.Email, project.Name, entry.Title, entryUrl, unsubscribeUrl);
            });

            await Task.WhenAll(tasks);
        }
    }
}
