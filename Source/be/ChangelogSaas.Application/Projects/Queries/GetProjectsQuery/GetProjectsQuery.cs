using ChangelogSaas.Application.Common.DTOs.Project;
using MediatR;

namespace ChangelogSaas.Application.Projects.Queries.GetProjectsQuery
{
    public sealed record GetProjectsQuery(GetProjectsRequest Request) : IRequest<List<ProjectDTO>>;
}
