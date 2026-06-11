using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Subscribers.Queries.GetSubscriberCountQuery
{
    public class GetSubscriberCountQueryHandler : IRequestHandler<GetSubscriberCountQuery, int>
    {
        private readonly IAppDbContext _db;

        public GetSubscriberCountQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<int> Handle(GetSubscriberCountQuery request, CancellationToken cancellationToken)
        {
            return await _db.Subscribers
                .Where(s => s.ProjectId == request.ProjectId && s.Status == SubscriberStatus.Verified)
                .CountAsync(cancellationToken);
        }
    }
}
