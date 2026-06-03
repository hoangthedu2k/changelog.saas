using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public CreateProjectCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var slug = string.IsNullOrWhiteSpace(request.Request.Slug)
                ? Project.Slugify(request.Request.Name)
                : request.Request.Slug.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(slug))
                throw new ValidationException("Cannot generate slug from project name. Please provide a slug explicitly.");

            var slugExists = await _db.Projects.AnyAsync(p => p.Slug == slug, cancellationToken);
            if (slugExists)
                throw new ValidationException($"Slug '{slug}' is already in use.");

            var project = Project.Create(request.UserId, request.Request.Name, slug);
            _db.Projects.Add(project);
            await _db.SaveChangesAsync(cancellationToken);
            return project.Id;
        }
    }
}
