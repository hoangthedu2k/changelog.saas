import { SubscriptionPlan } from '../services/billing.service';

export const PlanLimits = {
  maxProjects: (plan: SubscriptionPlan) => (plan === 'Team' ? Infinity : plan === 'Pro' ? 3 : 1),
  maxEntries: (plan: SubscriptionPlan) => (plan === 'Free' ? 5 : Infinity),
  maxSubscribers: (plan: SubscriptionPlan) => (plan === 'Team' ? Infinity : plan === 'Pro' ? 2000 : 100),
};
