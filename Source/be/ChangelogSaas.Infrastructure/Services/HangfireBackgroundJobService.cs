using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Infrastructure.Jobs;
using Hangfire;

namespace ChangelogSaas.Infrastructure.Services
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public void EnqueueNotifySubscribers(Guid entryId)
        {
            BackgroundJob.Enqueue<NotifySubscribersJob>(j => j.ExecuteAsync(entryId));
        }
    }
}
