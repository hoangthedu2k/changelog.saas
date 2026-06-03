using FluentValidation;

namespace ChangelogSaas.Application.Projects.Commands.DeleteProjectCommand
{
    public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
    {
        public DeleteProjectCommandValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("Project id is required.");
        }
    }
}
