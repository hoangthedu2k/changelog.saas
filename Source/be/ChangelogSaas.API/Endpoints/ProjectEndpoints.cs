using System.Security.Claims;
using ChangelogSaas.API.Extensions;
using ChangelogSaas.Application.Common.DTOs.Project;
using ChangelogSaas.Application.Projects.Commands.CreateProjectCommand;
using ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand;
using ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand;
using ChangelogSaas.Application.Projects.Queries.GetProjectsQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class ProjectEndpoints
    {
        public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/projects").RequireAuthorization();
            g.MapGet("/", GetList);
            g.MapPost("/", Create);
            g.MapPut("/{projectId:guid}", Update);
            g.MapDelete("/{projectId:guid}", Delete);
            return app;

        }
        private static async Task<IResult> GetList([AsParameters] GetProjectsRequest request, ISender sender, CancellationToken cancellationToken)
        {

            var result = await sender.Send(new GetProjectsQuery(request), cancellationToken);
            return Results.Ok(result);
        }
        private static async Task<IResult> Create(CreateProjectRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new CreateProjectCommand(user.GetUserId(), request), cancellationToken);
            return Results.Ok(result);
        }
        private static async Task<IResult> Update([FromRoute] Guid projectId, UpdateProjectRequest request, ISender sender, CancellationToken cancellationToken)
        {
            request.Id = projectId;
            var result = await sender.Send(new UpdateProjectCommand(request), cancellationToken);
            return Results.Ok(result);
        }
        private static async Task<IResult> Delete([FromRoute] Guid projectId, ISender sender, CancellationToken cancellationToken)
        {
            await sender.Send(new DeleteProjectCommand(projectId), cancellationToken);
            return Results.NoContent();
        }
    }
}
