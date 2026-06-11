using ChangelogSaas.Application.Common.DTOs.Widget;
using MediatR;

namespace ChangelogSaas.Application.Widget.Queries.GetWidgetByDomainQuery
{
    public sealed record GetWidgetByDomainQuery(string Host) : IRequest<WidgetResponse>;
}
