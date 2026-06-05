using ChangelogSaas.Application.Common.DTOs.Entry;
using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.UpdateEntryCommand
{
    public sealed record UpdateEntryCommand(UpdateEntryRequest Request) : IRequest<Guid>;
}
