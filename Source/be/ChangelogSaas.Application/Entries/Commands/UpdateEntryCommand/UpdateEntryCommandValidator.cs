using FluentValidation;

namespace ChangelogSaas.Application.Entries.Commands.UpdateEntryCommand
{
    public class UpdateEntryCommandValidator : AbstractValidator<UpdateEntryCommand>
    {
        public UpdateEntryCommandValidator()
        {
            RuleFor(x => x.Request.Id)
                .NotEmpty().WithMessage("Entry id is required.");

            RuleFor(x => x.Request.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

            RuleFor(x => x.Request.ContentHtml)
                .NotEmpty().WithMessage("Content is required.");

            RuleFor(x => x.Request.Version)
                .MaximumLength(50).WithMessage("Version must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Version));
        }
    }
}
