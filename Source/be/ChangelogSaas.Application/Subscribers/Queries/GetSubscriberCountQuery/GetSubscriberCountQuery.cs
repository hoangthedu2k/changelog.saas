using MediatR;

namespace ChangelogSaas.Application.Subscribers.Queries.GetSubscriberCountQuery
{
    public sealed record GetSubscriberCountQuery(Guid ProjectId) : IRequest<int>;
}
