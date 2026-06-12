using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Billing
{
    public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionDto>
    {
        private readonly IAppDbContext _db;

        public GetSubscriptionQueryHandler(IAppDbContext db) => _db = db;

        public async Task<SubscriptionDto> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

            var isTrialing = user?.IsInTrial ?? false;
            var trialPlan = isTrialing ? user!.TrialPlan : null;
            var trialDaysLeft = isTrialing
                ? Math.Max(0, (int)Math.Ceiling((user!.TrialEndsAt!.Value - DateTime.UtcNow).TotalDays))
                : 0;

            // Active paid subscription takes precedence over trial
            if (sub is not null && sub.Status != SubscriptionStatus.Canceled)
                return new SubscriptionDto(sub.Plan, sub.Status, sub.CurrentPeriodEnd, user?.StripeCustomerId, false, null, 0);

            var effectivePlan = isTrialing && trialPlan.HasValue ? trialPlan.Value : SubscriptionPlan.Free;
            return new SubscriptionDto(effectivePlan, SubscriptionStatus.Active, null, user?.StripeCustomerId, isTrialing, trialPlan, trialDaysLeft);
        }
    }
}
