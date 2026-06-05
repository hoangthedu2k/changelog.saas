namespace ChangelogSaas.Application.Common.DTOs.Entry
{
    public class CreateEntryRequest
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = "";
        public string ContentHtml { get; set; } = "";
        public List<string> Tags { get; set; } = [];
        public string? Version { get; set; }
    }
}
