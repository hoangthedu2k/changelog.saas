using ChangelogSaas.Application.Common;
using MediatR;

namespace ChangelogSaas.Application.Auth.Commands.OAuthLogin
{
    public sealed record OAuthLoginCommand(string Provider, string Token) : IRequest<AuthResult>;
}
