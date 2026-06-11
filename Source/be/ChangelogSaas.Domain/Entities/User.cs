namespace ChangelogSaas.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; } = "";
        public string PasswordHash { get; private set; } = "";
        public string? DisplayName { get; private set; }
        public string? StripeCustomerId { get; private set; }

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
