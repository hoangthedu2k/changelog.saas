using ChangelogSaas.Application.Common.DTOs.Project;
using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using ChangelogSaas.Domain.Plans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Projects.Commands.CreateProjectCommand
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDTO>
    {
        private readonly IAppDbContext _db;

        public CreateProjectCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<ProjectDTO> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);
            var plan = subscription?.Plan ?? SubscriptionPlan.Free;
            var inTrial = user?.IsInTrial ?? false;

            var projectCount = await _db.Projects.CountAsync(p => p.UserId == request.UserId, cancellationToken);
            if (projectCount >= PlanLimits.MaxProjects(plan, inTrial))
                throw new PlanLimitException($"Your {plan} plan allows up to {PlanLimits.MaxProjects(plan, inTrial)} project(s). Upgrade to create more.");

            var slug = string.IsNullOrWhiteSpace(request.Request.Slug)
                ? Project.Slugify(request.Request.Name)
                : request.Request.Slug.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(slug))
                throw new ValidationException("Cannot generate slug from project name. Please provide a slug explicitly.");

            var slugExists = await _db.Projects.AnyAsync(p => p.Slug == slug, cancellationToken);
            if (slugExists)
                throw new ValidationException($"Slug '{slug}' is already in use.");

            var project = Project.Create(request.UserId, request.Request.Name, slug);
            _db.Projects.Add(project);
            await _db.SaveChangesAsync(cancellationToken);

            return new ProjectDTO
            {
                Id = project.Id,
                UserId = project.UserId,
                Name = project.Name,
                Slug = project.Slug,
                AccentColor = project.AccentColor,
                IsPublic = project.IsPublic,
                WidgetPosition = project.WidgetPosition,
            };
        }
    }
}
