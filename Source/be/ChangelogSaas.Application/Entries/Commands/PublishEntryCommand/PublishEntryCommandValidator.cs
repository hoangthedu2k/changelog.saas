using FluentValidation;

namespace ChangelogSaas.Application.Entries.Commands.PublishEntryCommand
{
    public class PublishEntryCommandValidator : AbstractValidator<PublishEntryCommand>
    {
        public PublishEntryCommandValidator()
        {
            RuleFor(x => x.EntryId)
                .NotEmpty().WithMessage("Entry id is required.");
        }
    }
}
