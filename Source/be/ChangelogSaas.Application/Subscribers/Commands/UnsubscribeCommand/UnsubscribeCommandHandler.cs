using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Subscribers.Commands.UnsubscribeCommand
{
    public class UnsubscribeCommandHandler : IRequestHandler<UnsubscribeCommand>
    {
        private readonly IAppDbContext _db;

        public UnsubscribeCommandHandler(IAppDbContext db) => _db = db;

        public async Task Handle(UnsubscribeCommand request, CancellationToken cancellationToken)
        {
            var subscriber = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == request.Token, cancellationToken);

            if (subscriber is null)
                throw new NotFoundException(nameof(Subscriber), request.Token);

            subscriber.Unsubscribe();
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
