using ChangelogSaas.Application.Widget.Queries.GetWidgetByDomainQuery;
using ChangelogSaas.Application.Widget.Queries.GetWidgetQuery;
using MediatR;

namespace ChangelogSaas.API.Endpoints
{
    public static class WidgetEndpoints
    {
        public static IEndpointRouteBuilder MapWidgetEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/widget/{slug}", async (string slug, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(new GetWidgetQuery(slug), ct)))
               .RequireCors("Widget")
               .WithTags("Widget");

            // Resolve widget data by custom domain — reads Host header
            app.MapGet("/api/widget/by-domain", async (HttpContext ctx, ISender sender, CancellationToken ct) =>
            {
                var host = ctx.Request.Host.Value ?? "";
                return Results.Ok(await sender.Send(new GetWidgetByDomainQuery(host), ct));
            }).RequireCors("Widget")
              .WithTags("Widget");

            return app;
        }
    }
}
