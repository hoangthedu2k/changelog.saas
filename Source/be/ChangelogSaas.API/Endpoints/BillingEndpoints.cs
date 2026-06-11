using System.Security.Claims;
using ChangelogSaas.API.Extensions;
using ChangelogSaas.Application.Billing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChangelogSaas.API.Endpoints
{
    public static class BillingEndpoints
    {
        public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/billing");

            g.MapGet("/subscription", async (ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetSubscriptionQuery(user.GetUserId()), ct)))
            .RequireAuthorization();

            g.MapPost("/checkout", async (
                [FromBody] CheckoutRequest req,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken ct) =>
            {
                var url = await sender.Send(new CreateCheckoutCommand(
                    user.GetUserId(), req.PriceId, req.SuccessUrl, req.CancelUrl), ct);
                return Results.Ok(new { url });
            }).RequireAuthorization();

            g.MapPost("/portal", async (
                [FromBody] PortalRequest req,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken ct) =>
            {
                var sub = await sender.Send(new GetSubscriptionQuery(user.GetUserId()), ct);
                if (sub.StripeCustomerId is null)
                    return Results.BadRequest("No Stripe customer found. Please subscribe first.");

                var url = await sender.Send(
                    new CreatePortalCommand(sub.StripeCustomerId, req.ReturnUrl), ct);
                return Results.Ok(new { url });
            }).RequireAuthorization();

            // Stripe calls this — no JWT auth, verified by webhook signature
            g.MapPost("/webhook", async (HttpContext ctx, ISender sender) =>
            {
                var json = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
                var sig = ctx.Request.Headers["Stripe-Signature"].ToString();
                await sender.Send(new HandleStripeWebhookCommand(json, sig));
                return Results.Ok();
            });

            return app;
        }
    }

    public record CheckoutRequest(string PriceId, string SuccessUrl, string CancelUrl);
    public record PortalRequest(string ReturnUrl);
}
