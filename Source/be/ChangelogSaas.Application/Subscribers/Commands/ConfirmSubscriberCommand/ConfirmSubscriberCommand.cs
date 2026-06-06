using MediatR;

namespace ChangelogSaas.Application.Subscribers.Commands.ConfirmSubscriberCommand
{
    public sealed record ConfirmSubscriberCommand(string Token) : IRequest;
}
