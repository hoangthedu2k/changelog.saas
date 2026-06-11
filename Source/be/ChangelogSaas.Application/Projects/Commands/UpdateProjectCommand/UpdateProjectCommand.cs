using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public sealed record UpdateProjectCommand(UpdateProjectRequest Request) : IRequest<ProjectDTO>;
}
