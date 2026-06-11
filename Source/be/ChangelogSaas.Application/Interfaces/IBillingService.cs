namespace ChangelogSaas.Application.Interfaces
{
    public interface IBillingService
    {
        Task<string> CreateCheckoutSessionAsync(string userId, string priceId, string successUrl, string cancelUrl);
        Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl);
        Task HandleWebhookAsync(string json, string signature, CancellationToken cancellationToken = default);
    }
}
