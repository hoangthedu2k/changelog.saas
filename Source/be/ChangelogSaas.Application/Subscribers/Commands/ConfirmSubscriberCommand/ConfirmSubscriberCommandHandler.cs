using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Subscribers.Commands.ConfirmSubscriberCommand
{
    public class ConfirmSubscriberCommandHandler : IRequestHandler<ConfirmSubscriberCommand>
    {
        private readonly IAppDbContext _db;

        public ConfirmSubscriberCommandHandler(IAppDbContext db) => _db = db;

        public async Task Handle(ConfirmSubscriberCommand request, CancellationToken cancellationToken)
        {
            var subscriber = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.ConfirmToken == request.Token, cancellationToken);

            if (subscriber is null)
                throw new NotFoundException(nameof(Subscriber), request.Token);

            subscriber.Confirm();
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
