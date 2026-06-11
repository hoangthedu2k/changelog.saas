using ChangelogSaas.Application.Common.DTOs.Entry;
using ChangelogSaas.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Entries.Queries.GetEntryByIdQuery
{
    public class GetEntryByIdQueryHandler : IRequestHandler<GetEntryByIdQuery, EntryDTO?>
    {
        private readonly IAppDbContext _db;

        public GetEntryByIdQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<EntryDTO?> Handle(GetEntryByIdQuery request, CancellationToken cancellationToken)
        {
            return await _db.ChangelogEntries
                .Where(e => e.Id == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
