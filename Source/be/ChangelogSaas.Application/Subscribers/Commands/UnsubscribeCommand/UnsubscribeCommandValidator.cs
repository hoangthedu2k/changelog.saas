using FluentValidation;

namespace ChangelogSaas.Application.Subscribers.Commands.UnsubscribeCommand;

public class UnsubscribeCommandValidator : AbstractValidator<UnsubscribeCommand>
{
    public UnsubscribeCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Unsubscribe token is required.");
    }
}
