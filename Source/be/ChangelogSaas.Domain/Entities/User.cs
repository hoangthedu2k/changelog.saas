using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = "";
        public string PasswordHash { get; private set; } = "";
        public string? DisplayName { get; private set; }
        public string? StripeCustomerId { get; private set; }
        public SubscriptionPlan? TrialPlan { get; private set; }
        public DateTime? TrialEndsAt { get; private set; }

        public bool IsInTrial => TrialEndsAt.HasValue && DateTime.UtcNow < TrialEndsAt.Value;
        public SubscriptionPlan EffectivePlan => IsInTrial && TrialPlan.HasValue ? TrialPlan.Value : SubscriptionPlan.Free;

        public static User Create(string email, string passwordHash, string? displayName = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateDisplayName(string displayName) => DisplayName = displayName;

        public void SetStripeCustomerId(string customerId) => StripeCustomerId = customerId;

        public void StartTrial(SubscriptionPlan plan)
        {
            TrialPlan = plan;
            if (!TrialEndsAt.HasValue)
                TrialEndsAt = DateTime.UtcNow.AddDays(14);
            // Upgrading trial plan keeps the original expiry — clock does not reset
        }
    }
}
