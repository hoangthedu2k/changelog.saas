using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Plans
{
    public static class PlanLimits
    {
        public static int MaxProjects(SubscriptionPlan plan, bool inTrial = false) => (inTrial || plan == SubscriptionPlan.Pro || plan == SubscriptionPlan.Team) ? (plan == SubscriptionPlan.Team ? int.MaxValue : 3) : 1;

        public static int MaxEntries(SubscriptionPlan plan, bool inTrial = false) =>
            inTrial || plan is SubscriptionPlan.Pro or SubscriptionPlan.Team ? int.MaxValue : 5;

        public static int MaxSubscribers(SubscriptionPlan plan, bool inTrial = false) => plan switch
        {
            SubscriptionPlan.Team => int.MaxValue,
            SubscriptionPlan.Pro => 2000,
            _ => inTrial ? 2000 : 100
        };

        public static bool CanUseCustomDomain(SubscriptionPlan plan, bool inTrial = false) =>
            inTrial || plan is SubscriptionPlan.Pro or SubscriptionPlan.Team;

        public static bool CanRemoveBranding(SubscriptionPlan plan, bool inTrial = false) =>
            inTrial || plan is SubscriptionPlan.Pro or SubscriptionPlan.Team;

        public static bool CanUseApi(SubscriptionPlan plan, bool inTrial = false) =>
            inTrial || plan is SubscriptionPlan.Team;
    }
}
