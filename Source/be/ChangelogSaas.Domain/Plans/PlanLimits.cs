using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Plans
{
    public static class PlanLimits
    {
        public static int MaxProjects(SubscriptionPlan plan) => plan switch
        {
            SubscriptionPlan.Pro  => 3,
            SubscriptionPlan.Team => int.MaxValue,
            _                    => 1   // Free
        };

        public static int MaxEntries(SubscriptionPlan plan) => plan switch
        {
            SubscriptionPlan.Pro  => int.MaxValue,
            SubscriptionPlan.Team => int.MaxValue,
            _                    => 5   // Free
        };

        public static int MaxSubscribers(SubscriptionPlan plan) => plan switch
        {
            SubscriptionPlan.Pro  => 2000,
            SubscriptionPlan.Team => int.MaxValue,
            _                    => 100  // Free
        };

        public static bool CanUseCustomDomain(SubscriptionPlan plan) =>
            plan is SubscriptionPlan.Pro or SubscriptionPlan.Team;

        public static bool CanRemoveBranding(SubscriptionPlan plan) =>
            plan is SubscriptionPlan.Pro or SubscriptionPlan.Team;

        public static bool CanUseApi(SubscriptionPlan plan) =>
            plan is SubscriptionPlan.Team;
    }
}
