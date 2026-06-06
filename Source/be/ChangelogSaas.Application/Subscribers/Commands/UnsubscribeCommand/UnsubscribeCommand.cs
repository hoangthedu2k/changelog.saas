using MediatR;

namespace ChangelogSaas.Application.Subscribers.Commands.UnsubscribeCommand
{
    public sealed record UnsubscribeCommand(string Token) : IRequest;
}
