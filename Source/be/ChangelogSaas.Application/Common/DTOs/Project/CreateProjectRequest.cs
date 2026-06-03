using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Application.Common.DTOs.Project
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? CustomDomain { get; set; }
        public string AccentColor { get; set; } = "#6366f1";
        public bool IsPublic { get; set; } = true;
        public WidgetPosition WidgetPosition { get; set; } = WidgetPosition.tl;
    }
}
