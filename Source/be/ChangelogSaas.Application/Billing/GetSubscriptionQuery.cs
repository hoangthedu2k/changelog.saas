using ChangelogSaas.Domain.Enums;
using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public sealed record GetSubscriptionQuery(Guid UserId) : IRequest<SubscriptionDto>;

    public record SubscriptionDto(
        SubscriptionPlan Plan,
        SubscriptionStatus Status,
        DateTime? CurrentPeriodEnd,
        string? StripeCustomerId);
}
