using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;

namespace ChangelogSaas.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        public Guid ProjectId { get; private set; }
        public string Email { get; private set; } = "";
        public SubscriberStatus Status { get; private set; } = SubscriberStatus.Pending;
        public string? ConfirmToken { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public string? UnsubscribeToken { get; private set; }

        public static Subscriber Create(Guid projectId, string email)
        {
            return new Subscriber
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Email = email.Trim().ToLowerInvariant(),
                Status = SubscriberStatus.Pending,
                ConfirmToken = Guid.NewGuid().ToString("N"),
                UnsubscribeToken = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Confirm()
        {
            if (Status == SubscriberStatus.Verified)
                throw new DomainException("Subscriber already confirmed");
            Status = SubscriberStatus.Verified;
            ConfirmedAt = DateTime.UtcNow;
            ConfirmToken = null;
        }

        public void Unsubscribe()
        {
            Status = SubscriberStatus.Unsubscribed;
        }
    }
}
