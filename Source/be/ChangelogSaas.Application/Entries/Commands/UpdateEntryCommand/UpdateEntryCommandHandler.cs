using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.UpdateEntryCommand
{
    public class UpdateEntryCommandHandler : IRequestHandler<UpdateEntryCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public UpdateEntryCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Handle(UpdateEntryCommand request, CancellationToken cancellationToken)
        {
            var entry = await _db.ChangelogEntries
                .FindAsync(new object[] { request.Request.Id }, cancellationToken);
            if (entry is null)
                throw new NotFoundException(nameof(ChangelogEntry), request.Request.Id);

            var project = await _db.Projects
                .FindAsync(new object[] { entry.ProjectId }, cancellationToken);
            if (project is null || project.UserId != request.Request.UserId)
                throw new NotFoundException(nameof(ChangelogEntry), request.Request.Id);

            entry.UpdateContent(
                request.Request.Title,
                request.Request.ContentHtml,
                request.Request.Tags,
                request.Request.Version);

            await _db.SaveChangesAsync(cancellationToken);
            return entry.Id;
        }
    }
}
