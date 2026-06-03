using ChangelogSaas.Application.Common;
using MediatR;

namespace ChangelogSaas.Application.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
}
