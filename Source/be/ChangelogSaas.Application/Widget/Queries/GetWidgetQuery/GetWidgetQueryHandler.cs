using ChangelogSaas.Application.Common.DTOs.Widget;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Widget.Queries.GetWidgetQuery
{
    public class GetWidgetQueryHandler : IRequestHandler<GetWidgetQuery, WidgetResponse>
    {
        private readonly IAppDbContext _db;
        private readonly ICacheService _cache;

        public GetWidgetQueryHandler(IAppDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<WidgetResponse> Handle(GetWidgetQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"widget:{request.Slug}";

            var cached = await _cache.GetAsync<WidgetResponse>(cacheKey);
            if (cached is not null)
                return cached;

            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

            if (project is null)
                throw new NotFoundException(nameof(Project), request.Slug);

            var entries = await _db.ChangelogEntries
                .Where(e => e.ProjectId == project.Id && e.Status == EntryStatus.Published)
                .OrderByDescending(e => e.PublishedAt)
                .Select(e => new WidgetEntryDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    ContentHtml = e.ContentHtml,
                    Tags = e.Tags,
                    Version = e.Version,
                    PublishedAt = e.PublishedAt!.Value
                })
                .ToListAsync(cancellationToken);

            var response = new WidgetResponse
            {
                ProjectName = project.Name,
                Slug = project.Slug,
                AccentColor = project.AccentColor,
                WidgetPosition = project.WidgetPosition.ToString(),
                Entries = entries
            };

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }
    }
}
