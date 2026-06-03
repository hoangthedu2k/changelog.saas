using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public sealed record CreateProjectCommand(Guid UserId, CreateProjectRequest Request) : IRequest<Guid>;
}
