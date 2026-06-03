using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public UpdateProjectCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects.FindAsync(new object[] { request.Request.Id }, cancellationToken);
            if (project is null)
                throw new NotFoundException(nameof(Project), request.Request.Id);

            project.UpdateSettings(request.Request.Name, request.Request.AccentColor);
            project.SetCustomDomain(request.Request.CustomDomain);

            await _db.SaveChangesAsync(cancellationToken);
            return project.Id;
        }
    }
}
