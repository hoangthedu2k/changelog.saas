using ChangelogSaas.Application.Interfaces;
using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand>
    {
        private readonly IBillingService _billing;

        public HandleStripeWebhookCommandHandler(IBillingService billing)
        {
            _billing = billing;
        }

        public Task Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
        {
            return _billing.HandleWebhookAsync(request.Json, request.Signature, cancellationToken);
        }
    }
}
