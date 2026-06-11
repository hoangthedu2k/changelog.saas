using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public sealed record HandleStripeWebhookCommand(string Json, string Signature) : IRequest;
}
