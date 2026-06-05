using ChangelogSaas.Application.Common.DTOs.Entry;
using MediatR;

namespace ChangelogSaas.Application.Entries.Queries.GetEntriesQuery
{
    public sealed record GetEntriesQuery(GetEntriesRequest Request) : IRequest<List<EntryDTO>>;
}
