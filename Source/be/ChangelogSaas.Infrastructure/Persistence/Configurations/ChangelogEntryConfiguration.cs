using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangelogSaas.Infrastructure.Persistence.Configurations;

public sealed class ChangelogEntryConfiguration : IEntityTypeConfiguration<ChangelogEntry>
{
    public void Configure(EntityTypeBuilder<ChangelogEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Tags).HasColumnType("text[]");
        builder.Property(x => x.Version).HasMaxLength(50);
    }
}
