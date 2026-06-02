using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string StripeSubID { get; private set; } = "";
        public SubscriptionPlan Plan { get; private set; } = SubscriptionPlan.Free;
        public SubscriptionStatus Status { get; private set; }
        public DateTime? CurrentPeriodEnd { get; private set; }
        public DateTime? CancelAt { get; private set; }

        public static Subscription CreateFree(Guid userId)
        {
            return new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Plan = SubscriptionPlan.Free,
                Status = SubscriptionStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Upgrade(string stripeSubId, SubscriptionPlan plan, DateTime periodEnd)
        {
            StripeSubID = stripeSubId;
            Plan = plan;
            Status = SubscriptionStatus.Active;
            CurrentPeriodEnd = periodEnd;
        }

        public void Cancel(DateTime cancelAt)
        {
            Status = SubscriptionStatus.Canceled;
            CancelAt = cancelAt;
        }
    }
}
