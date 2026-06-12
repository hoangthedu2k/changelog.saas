using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangelogSaas.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100);
        builder.Property(x => x.StripeCustomerId).HasMaxLength(200);
        builder.Property(x => x.OAuthProvider).HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.OAuthProviderId).HasMaxLength(256).IsRequired(false);
        builder.Property(x => x.TrialPlan).IsRequired(false);
        builder.Property(x => x.TrialEndsAt).IsRequired(false);
        builder.Ignore(x => x.IsInTrial);
        builder.Ignore(x => x.EffectivePlan);
    }
}
