using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Project> Projects { get; }
        DbSet<Subscription> Subscriptions { get; }
        DbSet<Subscriber> Subscribers { get; }
        DbSet<EmailLogs> EmailLogs { get; }
        DbSet<ChangelogEntry> ChangelogEntries { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
