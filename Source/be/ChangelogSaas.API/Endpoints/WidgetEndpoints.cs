using ChangelogSaas.Application.Widget.Queries.GetWidgetQuery;
using MediatR;

namespace ChangelogSaas.API.Endpoints
{
    public static class WidgetEndpoints
    {
        public static IEndpointRouteBuilder MapWidgetEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/widget/{slug}", async (string slug, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(new GetWidgetQuery(slug), ct)));
            return app;
        }
    }
}
