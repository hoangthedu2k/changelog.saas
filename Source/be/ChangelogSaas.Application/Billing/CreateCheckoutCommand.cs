using MediatR;

namespace ChangelogSaas.Application.Billing
{
    public sealed record CreateCheckoutCommand(
        Guid UserId,
        string PriceId,
        string SuccessUrl,
        string CancelUrl) : IRequest<string>;
}
