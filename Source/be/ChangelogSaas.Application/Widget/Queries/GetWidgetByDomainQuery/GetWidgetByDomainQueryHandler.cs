using ChangelogSaas.Application.Common.DTOs.Widget;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Widget.Queries.GetWidgetByDomainQuery
{
    public class GetWidgetByDomainQueryHandler : IRequestHandler<GetWidgetByDomainQuery, WidgetResponse>
    {
        private readonly IAppDbContext _db;
        private readonly ICacheService _cache;

        public GetWidgetByDomainQueryHandler(IAppDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<WidgetResponse> Handle(GetWidgetByDomainQuery request, CancellationToken cancellationToken)
        {
            // Strip port from host header (e.g. "changelog.myapp.com:443" → "changelog.myapp.com")
            var host = request.Host.Split(':')[0].ToLowerInvariant();

            var cacheKey = $"widget:domain:{host}";

            var cached = await _cache.GetAsync<WidgetResponse>(cacheKey);
            if (cached is not null)
                return cached;

            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.CustomDomain != null && p.CustomDomain.ToLower() == host, cancellationToken);

            if (project is null)
                throw new NotFoundException(nameof(Project), host);

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
                ProjectId = project.Id,
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
