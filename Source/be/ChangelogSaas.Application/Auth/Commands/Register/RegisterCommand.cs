using ChangelogSaas.Application.Common;
using MediatR;

namespace ChangelogSaas.Application.Auth.Commands.Register
{
    public sealed record RegisterCommand(string Email, string Password, string? DisplayName) : IRequest<AuthResult>;
}
