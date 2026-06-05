using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Application.Common.DTOs.Entry
{
    public class EntryDTO
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = "";
        public string ContentHtml { get; set; } = "";
        public List<string> Tags { get; set; } = [];
        public EntryStatus Status { get; set; }
        public string? Version { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
