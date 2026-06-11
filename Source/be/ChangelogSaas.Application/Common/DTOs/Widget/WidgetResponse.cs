namespace ChangelogSaas.Application.Common.DTOs.Widget
{
    public class WidgetResponse
    {
        public Guid ProjectId { get; init; }
        public string ProjectName { get; init; } = "";
        public string Slug { get; init; } = "";
        public string AccentColor { get; init; } = "#6366f1";
        public string WidgetPosition { get; init; } = "";
        public List<WidgetEntryDto> Entries { get; init; } = [];
    }

    public class WidgetEntryDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string ContentHtml { get; init; } = "";
        public List<string> Tags { get; init; } = [];
        public string? Version { get; init; }
        public DateTime PublishedAt { get; init; }
    }
}
