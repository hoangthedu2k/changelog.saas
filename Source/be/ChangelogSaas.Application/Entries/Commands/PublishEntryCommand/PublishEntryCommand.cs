using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.PublishEntryCommand
{
    public sealed record PublishEntryCommand(Guid EntryId) : IRequest<Guid>;
}
