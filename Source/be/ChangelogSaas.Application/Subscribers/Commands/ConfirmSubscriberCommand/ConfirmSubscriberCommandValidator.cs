using FluentValidation;

namespace ChangelogSaas.Application.Subscribers.Commands.ConfirmSubscriberCommand;

public class ConfirmSubscriberCommandValidator : AbstractValidator<ConfirmSubscriberCommand>
{
    public ConfirmSubscriberCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Confirmation token is required.");
    }
}
