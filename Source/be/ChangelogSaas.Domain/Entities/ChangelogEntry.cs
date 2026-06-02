using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogSaas.Domain.Entities
{
    public class ChangelogEntry : BaseEntity
    {
        public Guid ProjectId { get; private set; }
        public string Title { get; private set; } = "";
        public string ContentHtml { get; private set; } = "";
        public List<string> Tags { get; private set; } = [];
        public EntryStatus Status { get; private set; }
        public string? Version { get; private set; }
        public DateTime? PublishedAt { get; private set; }

        // Factory method — không dùng constructor public
        public static ChangelogEntry Create(
            Guid projectId, string title, string html,
            List<string> tags, string? version)
        {
            return new ChangelogEntry
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = title,
                ContentHtml = html,
                Tags = tags,
                Version = version,
                Status = EntryStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Publish()
        {
            if (Status == EntryStatus.Published)
                throw new DomainException("Entry already published");
            Status = EntryStatus.Published;
            PublishedAt = DateTime.UtcNow;
        }
    }
}
