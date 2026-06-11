using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Billing
{
    public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionDto>
    {
        private readonly IAppDbContext _db;

        public GetSubscriptionQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<SubscriptionDto> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

            var trialEndsAt = user?.TrialEndsAt ?? DateTime.UtcNow;
            var trialDaysLeft = Math.Max(0, (int)Math.Ceiling((trialEndsAt - DateTime.UtcNow).TotalDays));

            if (sub is null)
                return new SubscriptionDto(SubscriptionPlan.Free, SubscriptionStatus.Active, null, user?.StripeCustomerId, trialEndsAt, trialDaysLeft);

            return new SubscriptionDto(sub.Plan, sub.Status, sub.CurrentPeriodEnd, user?.StripeCustomerId, trialEndsAt, trialDaysLeft);
        }
    }
}
