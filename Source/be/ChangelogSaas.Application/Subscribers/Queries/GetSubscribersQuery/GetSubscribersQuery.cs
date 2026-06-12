using MediatR;

namespace ChangelogSaas.Application.Subscribers.Queries.GetSubscribersQuery
{
    public sealed record SubscriberDto(
        Guid Id,
        string Email,
        string Status,
        DateTime CreatedAt,
        DateTime? ConfirmedAt);

    public sealed record PagedResult<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public sealed record GetSubscribersQuery(
        Guid ProjectId,
        Guid UserId,
        int Page = 1,
        int PageSize = 20,
        string? StatusFilter = null) : IRequest<PagedResult<SubscriberDto>>;
}
