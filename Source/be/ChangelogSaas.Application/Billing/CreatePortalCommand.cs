using ChangelogSaas.Application.Interfaces;
using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public sealed record CreatePortalCommand(string StripeCustomerId, string ReturnUrl) : IRequest<string>;

    public class CreatePortalCommandHandler : IRequestHandler<CreatePortalCommand, string>
    {
        private readonly IBillingService _billing;

        public CreatePortalCommandHandler(IBillingService billing)
        {
            _billing = billing;
        }

        public Task<string> Handle(CreatePortalCommand request, CancellationToken cancellationToken)
        {
            return _billing.CreatePortalSessionAsync(request.StripeCustomerId, request.ReturnUrl);
        }
    }
}
