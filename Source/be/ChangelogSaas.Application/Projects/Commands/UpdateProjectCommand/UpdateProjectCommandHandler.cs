using ChangelogSaas.Application.Common.DTOs.Project;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDTO>
    {
        private readonly IAppDbContext _db;

        public UpdateProjectCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<ProjectDTO> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects.FindAsync(new object[] { request.Request.Id }, cancellationToken);
            if (project is null)
                throw new NotFoundException(nameof(Project), request.Request.Id);
            if (project.UserId != request.Request.UserId)
                throw new NotFoundException(nameof(Project), request.Request.Id);

            project.Update(
                request.Request.Name,
                request.Request.Slug,
                request.Request.AccentColor,
                request.Request.IsPublic,
                request.Request.WidgetPosition,
                request.Request.CustomDomain);

            await _db.SaveChangesAsync(cancellationToken);

            return new ProjectDTO
            {
                Id = project.Id,
                UserId = project.UserId,
                Name = project.Name,
                Slug = project.Slug,
                AccentColor = project.AccentColor,
                IsPublic = project.IsPublic,
                WidgetPosition = project.WidgetPosition,
                CustomDomain = project.CustomDomain,
            };
        }
    }
}
