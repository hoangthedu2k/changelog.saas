using FluentValidation;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");

            RuleFor(x => x.Request.Slug)
                .MaximumLength(100).WithMessage("Project slug must not exceed 100 characters.")
                .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, digits, and dashes.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Slug));
        }
    }
}
