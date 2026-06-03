using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<EmailLogs> EmailLogs { get; set; }
        public DbSet<ChangelogEntry> ChangelogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
