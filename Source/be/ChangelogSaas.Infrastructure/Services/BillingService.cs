using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace ChangelogSaas.Infrastructure.Services
{
    public class BillingService : IBillingService
    {
        private readonly IAppDbContext _db;
        private readonly string _webhookSecret;
        private readonly string _proPriceId;
        private readonly string _teamPriceId;

        public BillingService(IAppDbContext db, IConfiguration config)
        {
            _db = db;
            _webhookSecret = config["Stripe:WebhookSecret"]
                ?? throw new InvalidOperationException("Stripe:WebhookSecret is missing.");
            _proPriceId = config["Stripe:ProPriceId"] ?? "";
            _teamPriceId = config["Stripe:TeamPriceId"] ?? "";

            StripeConfiguration.ApiKey = config["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe:SecretKey is missing.");
        }

        public async Task<string> CreateCheckoutSessionAsync(
            string userId, string? existingStripeCustomerId, string priceId, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = userId,
                Customer = existingStripeCustomerId,
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string> { ["userId"] = userId }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl)
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl,
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task HandleWebhookAsync(string json, string signature, CancellationToken cancellationToken = default)
        {
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);
            }
            catch (StripeException)
            {
                throw new UnauthorizedAccessException("Invalid Stripe webhook signature.");
            }

            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionCreated:
                case EventTypes.CustomerSubscriptionUpdated:
                    await HandleSubscriptionUpsert((Stripe.Subscription)stripeEvent.Data.Object, cancellationToken);
                    break;

                case EventTypes.CustomerSubscriptionDeleted:
                    await HandleSubscriptionDeleted((Stripe.Subscription)stripeEvent.Data.Object, cancellationToken);
                    break;
            }
        }

        private async Task HandleSubscriptionUpsert(Stripe.Subscription stripeSub, CancellationToken ct)
        {
            var userId = GetUserId(stripeSub);
            if (userId == Guid.Empty) return;

            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (sub is null)
            {
                sub = Domain.Entities.Subscription.CreateFree(userId);
                _db.Subscriptions.Add(sub);
            }

            var firstItem = stripeSub.Items.Data.FirstOrDefault();
            var plan = ResolvePlan(firstItem?.Price?.Id);
            var periodEnd = firstItem?.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1);
            sub.Upgrade(stripeSub.Id, plan, periodEnd);

            var user = await _db.Users.FindAsync(new object[] { userId }, ct);
            if (user is not null && user.StripeCustomerId is null)
                user.SetStripeCustomerId(stripeSub.CustomerId);

            // Unlock all projects on upgrade
            var projects = await _db.Projects.Where(p => p.UserId == userId).ToListAsync(ct);
            foreach (var p in projects) p.Unlock();

            await _db.SaveChangesAsync(ct);
        }

        private async Task HandleSubscriptionDeleted(Stripe.Subscription stripeSub, CancellationToken ct)
        {
            var userId = GetUserId(stripeSub);
            if (userId == Guid.Empty) return;

            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (sub is null) return;

            sub.Cancel(stripeSub.CanceledAt ?? DateTime.UtcNow);

            // Lock excess projects beyond Free limit (keep oldest 1 unlocked)
            var freeLimit = Domain.Plans.PlanLimits.MaxProjects(SubscriptionPlan.Free);
            var projects = await _db.Projects
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(ct);

            for (var i = 0; i < projects.Count; i++)
            {
                if (i < freeLimit) projects[i].Unlock();
                else projects[i].Lock();
            }

            await _db.SaveChangesAsync(ct);
        }

        private static Guid GetUserId(Stripe.Subscription stripeSub)
        {
            stripeSub.Metadata.TryGetValue("userId", out var raw);
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }

        private SubscriptionPlan ResolvePlan(string? priceId) => priceId switch
        {
            var p when p == _teamPriceId => SubscriptionPlan.Team,
            var p when p == _proPriceId => SubscriptionPlan.Pro,
            _ => SubscriptionPlan.Pro  // fallback for any paid price
        };
    }
}
