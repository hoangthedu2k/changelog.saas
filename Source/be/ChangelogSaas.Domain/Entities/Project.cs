using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Entities
{
    public class Project : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = "";
        public string Slug { get; private set; } = "";
        public string? CustomDomain { get; private set; }
        public string AccentColor { get; private set; } = "#6366f1";
        public bool IsPublic { get; private set; } = true;
        public WidgetPosition WidgetPosition { get; private set; }

        public static Project Create(Guid userId, string name, string slug)
        {
            return new Project
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Slug = slug.ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateSettings(string name, string color)
        {
            Name = name;
            AccentColor = color;
        }

        public void SetCustomDomain(string? domain)
        {
            CustomDomain = domain;
        }
    }
}
