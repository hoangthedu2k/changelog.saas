using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.DeleteEntryCommand
{
    public class DeleteEntryCommandHandler : IRequestHandler<DeleteEntryCommand>
    {
        private readonly IAppDbContext _db;

        public DeleteEntryCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteEntryCommand request, CancellationToken cancellationToken)
        {
            var entry = await _db.ChangelogEntries
                .FindAsync(new object[] { request.EntryId }, cancellationToken);
            if (entry is null)
                throw new NotFoundException(nameof(ChangelogEntry), request.EntryId);

            _db.ChangelogEntries.Remove(entry);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
