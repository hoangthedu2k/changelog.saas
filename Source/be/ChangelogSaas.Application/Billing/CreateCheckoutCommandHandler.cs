using ChangelogSaas.Application.Interfaces;
using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public class CreateCheckoutCommandHandler : IRequestHandler<CreateCheckoutCommand, string>
    {
        private readonly IBillingService _billing;
        private readonly IAppDbContext _db;

        public CreateCheckoutCommandHandler(IBillingService billing, IAppDbContext db)
        {
            _billing = billing;
            _db = db;
        }

        public async Task<string> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            return await _billing.CreateCheckoutSessionAsync(
                request.UserId.ToString(),
                user?.StripeCustomerId,
                request.PriceId,
                request.SuccessUrl,
                request.CancelUrl);
        }
    }
}
