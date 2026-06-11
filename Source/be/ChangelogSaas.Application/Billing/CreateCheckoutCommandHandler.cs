using ChangelogSaas.Application.Interfaces;
using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public class CreateCheckoutCommandHandler : IRequestHandler<CreateCheckoutCommand, string>
    {
        private readonly IBillingService _billing;

        public CreateCheckoutCommandHandler(IBillingService billing)
        {
            _billing = billing;
        }

        public Task<string> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken)
        {
            return _billing.CreateCheckoutSessionAsync(
                request.UserId.ToString(),
                request.PriceId,
                request.SuccessUrl,
                request.CancelUrl);
        }
    }
}
