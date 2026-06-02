using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
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

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.PasswordHash).IsRequired();
                e.Property(x => x.DisplayName).HasMaxLength(100);
            });

            // Project
            modelBuilder.Entity<Project>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.Slug).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Slug).IsUnique();
                e.Property(x => x.AccentColor).HasMaxLength(20);
                e.Property(x => x.CustomDomain).HasMaxLength(253);
            });

            // ChangelogEntry — Tags dùng text[] của PostgreSQL
            modelBuilder.Entity<ChangelogEntry>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(300);
                e.Property(x => x.Tags).HasColumnType("text[]");
                e.Property(x => x.Version).HasMaxLength(50);
            });

            // Subscriber
            modelBuilder.Entity<Subscriber>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);
                e.HasIndex(x => new { x.ProjectId, x.Email }).IsUnique();
            });

            // Subscription
            modelBuilder.Entity<Subscription>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.StripeSubID).HasMaxLength(200);
            });

            // EmailLogs
            modelBuilder.Entity<EmailLogs>(e =>
            {
                e.HasKey(x => x.Id);
            });
        }
    }
}
