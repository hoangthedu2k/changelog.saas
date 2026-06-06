using MediatR;

namespace ChangelogSaas.Application.Subscribers.Commands.SubscribeCommand
{
    public sealed record SubscribeCommand(Guid ProjectId, string Email) : IRequest<string>;
}
