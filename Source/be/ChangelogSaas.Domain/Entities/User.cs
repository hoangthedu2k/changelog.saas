namespace ChangelogSaas.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = "";
        public string PasswordHash { get; private set; } = "";
        public string? DisplayName { get; private set; }
        public string? StripeCustomerId { get; private set; }
        public DateTime TrialEndsAt { get; private set; }

        public bool IsInTrial => DateTime.UtcNow < TrialEndsAt;

        public static User Create(string email, string passwordHash, string? displayName = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                DisplayName = displayName,
                TrialEndsAt = DateTime.UtcNow.AddDays(14),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public void SetStripeCustomerId(string customerId)
        {
            StripeCustomerId = customerId;
        }
    }
}
