using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public sealed record StartTrialCommand(Guid UserId, SubscriptionPlan Plan) : IRequest;

    public class StartTrialCommandHandler : IRequestHandler<StartTrialCommand>
    {
        private readonly IAppDbContext _db;

        public StartTrialCommandHandler(IAppDbContext db) => _db = db;

        public async Task Handle(StartTrialCommand request, CancellationToken cancellationToken)
        {
            if (request.Plan == SubscriptionPlan.Free)
                throw new ValidationException("Cannot start a trial for Free plan.");

            var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken)
                ?? throw new NotFoundException("User", request.UserId);

            if (user.IsInTrial)
                throw new ValidationException("You already have an active trial.");

            var hasPaidSub = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .AnyAsync(_db.Subscriptions, s => s.UserId == request.UserId, cancellationToken);
            if (hasPaidSub)
                throw new ValidationException("You already have an active subscription.");

            user.StartTrial(request.Plan);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
