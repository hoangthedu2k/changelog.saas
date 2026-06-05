using FluentValidation;

namespace ChangelogSaas.Application.Entries.Commands.CreateEntryCommand
{
    public class CreateEntryCommandValidator : AbstractValidator<CreateEntryCommand>
    {
        public CreateEntryCommandValidator()
        {
            RuleFor(x => x.Request.ProjectId)
                .NotEmpty().WithMessage("ProjectId is required.");

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
