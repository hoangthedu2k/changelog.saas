using ChangelogSaas.Application.Common.DTOs.Widget;
using MediatR;

namespace ChangelogSaas.Application.Widget.Queries.GetWidgetQuery
{
    public sealed record GetWidgetQuery(string Slug) : IRequest<WidgetResponse>;
}
