using System.Text;
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

        public static string Slugify(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";

            var sb = new StringBuilder(name.Length);
            var prevDash = false;
            foreach (var c in name.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    prevDash = false;
                }
                else if (!prevDash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevDash = true;
                }
            }

            return sb.ToString().TrimEnd('-');
        }

        public void Update(string name, string slug, string accentColor, bool isPublic, WidgetPosition widgetPosition, string? customDomain)
        {
            Name = name;
            Slug = slug.ToLowerInvariant();
            AccentColor = accentColor;
            IsPublic = isPublic;
            WidgetPosition = widgetPosition;
            CustomDomain = customDomain;
        }
    }
}
