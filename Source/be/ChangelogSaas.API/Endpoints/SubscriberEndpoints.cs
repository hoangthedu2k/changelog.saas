using System.Security.Claims;
using ChangelogSaas.API.Extensions;
using ChangelogSaas.Application.Subscribers.Commands.ConfirmSubscriberCommand;
using ChangelogSaas.Application.Subscribers.Commands.SubscribeCommand;
using ChangelogSaas.Application.Subscribers.Commands.UnsubscribeCommand;
using ChangelogSaas.Application.Subscribers.Queries.GetSubscriberCountQuery;
using ChangelogSaas.Application.Subscribers.Queries.GetSubscribersQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class SubscriberEndpoints
    {
        public static IEndpointRouteBuilder MapSubscriberEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/subscribers");

            g.MapGet("/", async (
                [FromQuery] Guid projectId,
                [FromQuery] int page,
                [FromQuery] int pageSize,
                [FromQuery] string? status,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken ct) =>
                Results.Ok(await sender.Send(
                    new GetSubscribersQuery(projectId, user.GetUserId(),
                        page == 0 ? 1 : page,
                        pageSize == 0 ? 20 : pageSize,
                        status), ct)))
            .RequireAuthorization();

            g.MapGet("/count", async ([FromQuery] Guid projectId, ISender sender, CancellationToken ct) =>
            {
                var count = await sender.Send(new GetSubscriberCountQuery(projectId), ct);
                return Results.Ok(new { count });
            }).RequireAuthorization();

            g.MapPost("/", async (SubscribeCommand cmd, ISender sender, CancellationToken ct)
                => Results.Ok(new { confirmToken = await sender.Send(cmd, ct) }));

            g.MapGet("/confirm/{token}", async (string token, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ConfirmSubscriberCommand(token), ct);
                return Results.Ok(new { message = "Subscription confirmed." });
            });

            g.MapGet("/unsubscribe/{token}", async (string token, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UnsubscribeCommand(token), ct);
                return Results.Ok(new { message = "Unsubscribed successfully." });
            });

            return app;
        }
    }
}
