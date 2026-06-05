using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.PublishEntryCommand
{
    public class PublishEntryCommandHandler : IRequestHandler<PublishEntryCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public PublishEntryCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Handle(PublishEntryCommand request, CancellationToken cancellationToken)
        {
            var entry = await _db.ChangelogEntries
                .FindAsync(new object[] { request.EntryId }, cancellationToken);
            if (entry is null)
                throw new NotFoundException(nameof(ChangelogEntry), request.EntryId);

            entry.Publish();
            await _db.SaveChangesAsync(cancellationToken);
            return entry.Id;
        }
    }
}
