using FluentValidation;

namespace ChangelogSaas.Application.Entries.Commands.DeleteEntryCommand
{
    public class DeleteEntryCommandValidator : AbstractValidator<DeleteEntryCommand>
    {
        public DeleteEntryCommandValidator()
        {
            RuleFor(x => x.EntryId)
                .NotEmpty().WithMessage("Entry id is required.");
        }
    }
}
