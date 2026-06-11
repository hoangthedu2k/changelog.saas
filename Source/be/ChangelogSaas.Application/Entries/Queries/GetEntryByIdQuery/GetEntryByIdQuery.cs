using ChangelogSaas.Application.Common.DTOs.Entry;
using MediatR;

namespace ChangelogSaas.Application.Entries.Queries.GetEntryByIdQuery
{
    public sealed record GetEntryByIdQuery(Guid Id) : IRequest<EntryDTO?>;
}
