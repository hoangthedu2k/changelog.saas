using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Subscribers.Queries.GetSubscribersQuery
{
    public class GetSubscribersQueryHandler : IRequestHandler<GetSubscribersQuery, PagedResult<SubscriberDto>>
    {
        private readonly IAppDbContext _db;

        public GetSubscribersQueryHandler(IAppDbContext db) => _db = db;

        public async Task<PagedResult<SubscriberDto>> Handle(GetSubscribersQuery request, CancellationToken cancellationToken)
        {
            var exists = await _db.Projects
                .AnyAsync(p => p.Id == request.ProjectId && p.UserId == request.UserId, cancellationToken);
            if (!exists) throw new NotFoundException("Project", request.ProjectId);

            var query = _db.Subscribers.Where(s => s.ProjectId == request.ProjectId);

            if (!string.IsNullOrEmpty(request.StatusFilter) &&
                Enum.TryParse<SubscriberStatus>(request.StatusFilter, true, out var status))
            {
                query = query.Where(s => s.Status == status);
            }

            var total = await query.CountAsync(cancellationToken);

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubscriberDto(s.Id, s.Email, s.Status.ToString(), s.CreatedAt, s.ConfirmedAt))
                .ToListAsync(cancellationToken);

            return new PagedResult<SubscriberDto>(items, total, page, pageSize);
        }
    }
}
