using ChangelogSaas.Application.Common.DTOs.Entry;
using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.CreateEntryCommand
{
    public sealed record CreateEntryCommand(CreateEntryRequest Request) : IRequest<Guid>;
}
