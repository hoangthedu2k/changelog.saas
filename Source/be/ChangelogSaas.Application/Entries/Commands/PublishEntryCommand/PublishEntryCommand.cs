using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.PublishEntryCommand
{
    public sealed record PublishEntryCommand(Guid EntryId, Guid UserId) : IRequest<Guid>;
}
