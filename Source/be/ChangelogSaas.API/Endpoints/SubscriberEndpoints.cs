using ChangelogSaas.Application.Subscribers.Commands.ConfirmSubscriberCommand;
using ChangelogSaas.Application.Subscribers.Commands.SubscribeCommand;
using ChangelogSaas.Application.Subscribers.Commands.UnsubscribeCommand;
using ChangelogSaas.Application.Subscribers.Queries.GetSubscriberCountQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class SubscriberEndpoints
    {
        public static IEndpointRouteBuilder MapSubscriberEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/subscribers");

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
