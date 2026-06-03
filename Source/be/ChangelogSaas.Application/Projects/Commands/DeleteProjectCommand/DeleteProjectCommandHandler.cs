using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
    {
        private readonly IAppDbContext _db;

        public DeleteProjectCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects.FindAsync(new object[] { request.ProjectId }, cancellationToken);
            if (project is null)
                throw new NotFoundException(nameof(Project), request.ProjectId);

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
