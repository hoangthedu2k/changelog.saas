using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
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
            var projectExists = await _db.Projects
                .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
            if (!projectExists)
                throw new NotFoundException(nameof(Project), request.ProjectId);

            var email = request.Email.Trim().ToLowerInvariant();

            var existing = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.ProjectId == request.ProjectId && s.Email == email, cancellationToken);

            if (existing is not null)
            {
                if (existing.Status == SubscriberStatus.Verified)
                    throw new DomainException("Email is already subscribed.");
                // Pending: return the existing confirm token so they can resend
                return existing.ConfirmToken!;
            }

            var subscriber = Subscriber.Create(request.ProjectId, email);
            _db.Subscribers.Add(subscriber);
            await _db.SaveChangesAsync(cancellationToken);

            return subscriber.ConfirmToken!;
        }
    }
}
