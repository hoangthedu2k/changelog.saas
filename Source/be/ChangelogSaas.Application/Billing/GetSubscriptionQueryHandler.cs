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
            var sub = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

            var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (sub is null)
                return new SubscriptionDto(SubscriptionPlan.Free, SubscriptionStatus.Active, null, user?.StripeCustomerId);

            return new SubscriptionDto(sub.Plan, sub.Status, sub.CurrentPeriodEnd, user?.StripeCustomerId);
        }
    }
}
