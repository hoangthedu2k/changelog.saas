using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using ChangelogSaas.Domain.Plans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Subscribers.Commands.SubscribeCommand
{
    public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, string>
    {
        private readonly IAppDbContext _db;

        public SubscribeCommandHandler(IAppDbContext db) => _db = db;

        public async Task<string> Handle(SubscribeCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
            if (project is null)
                throw new NotFoundException(nameof(Project), request.ProjectId);

            var user = await _db.Users.FindAsync(new object[] { project.UserId }, cancellationToken);
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == project.UserId, cancellationToken);
            var plan = subscription?.Plan ?? SubscriptionPlan.Free;
            var inTrial = user?.IsInTrial ?? false;

            var subscriberCount = await _db.Subscribers
                .CountAsync(s => s.ProjectId == request.ProjectId && s.Status == SubscriberStatus.Verified, cancellationToken);
            if (subscriberCount >= PlanLimits.MaxSubscribers(plan, inTrial))
                throw new PlanLimitException($"This project has reached the subscriber limit for the {plan} plan. The owner must upgrade to accept more subscribers.");

            var email = request.Email.Trim().ToLowerInvariant();

            var existing = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.ProjectId == request.ProjectId && s.Email == email, cancellationToken);

            if (existing is not null)
            {
                if (existing.Status == SubscriberStatus.Verified)
                    throw new DomainException("Email is already subscribed.");
                return existing.ConfirmToken!;
            }

            var subscriber = Subscriber.Create(request.ProjectId, email);
            _db.Subscribers.Add(subscriber);
            await _db.SaveChangesAsync(cancellationToken);

            return subscriber.ConfirmToken!;
        }
    }
}
