using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Application.Common.DTOs.Entry
{
    public class GetEntriesRequest
    {
        public Guid ProjectId { get; init; }
        public EntryStatus? Status { get; init; }
        public string? Title { get; init; }
    }
}
