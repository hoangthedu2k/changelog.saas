using ChangelogSaas.Application.Common.DTOs.Project;
using ChangelogSaas.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Projects.Queries.GetProjectsQuery
{
    public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDTO>>
    {
        private readonly IAppDbContext _db;

        public GetProjectsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProjectDTO>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Projects.Where(p => p.UserId == request.UserId);

            if (!string.IsNullOrWhiteSpace(request.Request.Name))
                query = query.Where(p => p.Name.Contains(request.Request.Name!));

            if (!string.IsNullOrWhiteSpace(request.Request.Slug))
                query = query.Where(p => p.Slug.Contains(request.Request.Slug!));

            return await query
                .Select(p => new ProjectDTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Name = p.Name,
                    Slug = p.Slug,
                    CustomDomain = p.CustomDomain,
                    AccentColor = p.AccentColor,
                    IsPublic = p.IsPublic,
                    WidgetPosition = p.WidgetPosition,
                    IsLocked = p.IsLocked
                })
                .ToListAsync(cancellationToken);
        }
    }
}
