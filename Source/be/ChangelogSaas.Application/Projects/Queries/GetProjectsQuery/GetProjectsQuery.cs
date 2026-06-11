using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Queries.GetProjectsQuery
{
    public sealed record GetProjectsQuery(Guid UserId, GetProjectsRequest Request) : IRequest<List<ProjectDTO>>;
}
