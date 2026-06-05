using MediatR;

namespace ChangelogSaas.Application.Entries.Commands.DeleteEntryCommand
{
    public sealed record DeleteEntryCommand(Guid EntryId) : IRequest;
}
