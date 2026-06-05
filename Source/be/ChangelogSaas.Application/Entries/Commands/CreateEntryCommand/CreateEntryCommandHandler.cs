using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Entries.Commands.CreateEntryCommand
{
    public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public CreateEntryCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
        {
            var projectExists = await _db.Projects
                .AnyAsync(p => p.Id == request.Request.ProjectId, cancellationToken);
            if (!projectExists)
                throw new NotFoundException(nameof(Project), request.Request.ProjectId);

            var entry = ChangelogEntry.Create(
                request.Request.ProjectId,
                request.Request.Title,
                request.Request.ContentHtml,
                request.Request.Tags,
                request.Request.Version);

            _db.ChangelogEntries.Add(entry);
            await _db.SaveChangesAsync(cancellationToken);
            return entry.Id;
        }
    }
}
