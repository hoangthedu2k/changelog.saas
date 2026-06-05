using ChangelogSaas.Application.Common.DTOs.Entry;
using ChangelogSaas.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Entries.Queries.GetEntriesQuery
{
    public class GetEntriesQueryHandler : IRequestHandler<GetEntriesQuery, List<EntryDTO>>
    {
        private readonly IAppDbContext _db;

        public GetEntriesQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<EntryDTO>> Handle(GetEntriesQuery request, CancellationToken cancellationToken)
        {
            var query = _db.ChangelogEntries
                .Where(e => e.ProjectId == request.Request.ProjectId);

            if (request.Request.Status.HasValue)
                query = query.Where(e => e.Status == request.Request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.Request.Title))
                query = query.Where(e => e.Title.Contains(request.Request.Title!));

            return await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new EntryDTO
                {
                    Id = e.Id,
                    ProjectId = e.ProjectId,
                    Title = e.Title,
                    ContentHtml = e.ContentHtml,
                    Tags = e.Tags,
                    Status = e.Status,
                    Version = e.Version,
                    PublishedAt = e.PublishedAt,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
