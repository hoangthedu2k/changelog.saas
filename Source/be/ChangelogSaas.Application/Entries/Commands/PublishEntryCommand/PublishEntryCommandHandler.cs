using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.PublishEntryCommand
{
    public class PublishEntryCommandHandler : IRequestHandler<PublishEntryCommand, Guid>
    {
        private readonly IAppDbContext _db;
        private readonly ICacheService _cache;

        public PublishEntryCommandHandler(IAppDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Guid> Handle(PublishEntryCommand request, CancellationToken cancellationToken)
        {
            var entry = await _db.ChangelogEntries
                .FindAsync(new object[] { request.EntryId }, cancellationToken);
            if (entry is null)
                throw new NotFoundException(nameof(ChangelogEntry), request.EntryId);

            var project = await _db.Projects
                .FindAsync(new object[] { entry.ProjectId }, cancellationToken);
            if (project is null || project.UserId != request.UserId)
                throw new NotFoundException(nameof(ChangelogEntry), request.EntryId);

            entry.Publish();
            await _db.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync($"widget:{project.Slug}");

            return entry.Id;
        }
    }
}
