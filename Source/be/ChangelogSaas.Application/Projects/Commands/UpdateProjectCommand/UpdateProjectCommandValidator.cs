using FluentValidation;

namespace ChangelogSaas.Application.Projects.Commands.UpdateProjectCommand
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(x => x.Request.Id)
                .NotEmpty().WithMessage("Project id is required.");

            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");

            RuleFor(x => x.Request.AccentColor)
                .NotEmpty().WithMessage("Accent color is required.")
                .MaximumLength(20).WithMessage("Accent color must not exceed 20 characters.");

            RuleFor(x => x.Request.CustomDomain)
                .MaximumLength(255).WithMessage("Custom domain must not exceed 255 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.CustomDomain));
        }
    }
}
