using System.Security.Claims;
using ChangelogSaas.API.Extensions;
using ChangelogSaas.Application.Common.DTOs.Entry;
using ChangelogSaas.Application.Entries.Commands.CreateEntryCommand;
using ChangelogSaas.Application.Entries.Commands.DeleteEntryCommand;
using ChangelogSaas.Application.Entries.Commands.PublishEntryCommand;
using ChangelogSaas.Application.Entries.Commands.UpdateEntryCommand;
using ChangelogSaas.Application.Entries.Queries.GetEntriesQuery;
using ChangelogSaas.Application.Entries.Queries.GetEntryByIdQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class EntryEndpoints
    {
        public static IEndpointRouteBuilder MapEntryEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/entries").RequireAuthorization();
            g.MapGet("/", GetList);
            g.MapGet("/{entryId:guid}", GetById);
            g.MapPost("/", Create);
            g.MapPut("/{entryId:guid}", Update);
            g.MapDelete("/{entryId:guid}", Delete);
            g.MapPost("/{entryId:guid}/publish", Publish);
            return app;
        }

        private static async Task<IResult> GetList([AsParameters] GetEntriesRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetEntriesQuery(request), cancellationToken);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetById([FromRoute] Guid entryId, ISender sender, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetEntryByIdQuery(entryId), cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }

        private static async Task<IResult> Create(CreateEntryRequest request, ISender sender, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new CreateEntryCommand(request), cancellationToken);
            return Results.Ok(result);
        }

        private static async Task<IResult> Update([FromRoute] Guid entryId, UpdateEntryRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
        {
            request.Id = entryId;
            request.UserId = user.GetUserId();
            var result = await sender.Send(new UpdateEntryCommand(request), cancellationToken);
            return Results.Ok(result);
        }

        private static async Task<IResult> Delete([FromRoute] Guid entryId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
        {
            await sender.Send(new DeleteEntryCommand(entryId, user.GetUserId()), cancellationToken);
            return Results.NoContent();
        }

        private static async Task<IResult> Publish([FromRoute] Guid entryId, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new PublishEntryCommand(entryId, user.GetUserId()), cancellationToken);
            return Results.Ok(result);
        }
    }
}
