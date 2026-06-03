using ChangelogSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangelogSaas.Infrastructure.Persistence.Configurations;

public sealed class EmailLogsConfiguration : IEntityTypeConfiguration<EmailLogs>
{
    public void Configure(EntityTypeBuilder<EmailLogs> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
