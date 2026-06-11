using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand
{
    public sealed record DeleteProjectCommand(Guid ProjectId, Guid UserId) : IRequest;
}
