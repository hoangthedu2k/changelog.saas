using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Exceptions;
using ChangelogSaas.Domain.Plans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Entries.Commands.CreateEntryCommand
{
    public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public CreateEntryCommandHandler(IAppDbContext db) => _db = db;

        public async Task<Guid> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == request.Request.ProjectId, cancellationToken)
                ?? throw new NotFoundException(nameof(Project), request.Request.ProjectId);

            if (project.IsLocked)
                throw new DomainException("This project is locked. Upgrade your plan to create new entries.");

            var user = await _db.Users.FindAsync(new object[] { project.UserId }, cancellationToken);
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == project.UserId, cancellationToken);
            var plan = subscription?.Plan ?? user?.EffectivePlan ?? Domain.Enums.SubscriptionPlan.Free;

            var entryCount = await _db.ChangelogEntries.CountAsync(e => e.ProjectId == request.Request.ProjectId, cancellationToken);
            if (entryCount >= PlanLimits.MaxEntries(plan))
                throw new PlanLimitException($"Your {plan} plan allows up to {PlanLimits.MaxEntries(plan)} entries per project. Upgrade to create more.");

            var entry = ChangelogEntry.Create(
                request.Request.ProjectId,
                request.Request.Title,
                request.Request.ContentHtml,
                request.Request.Tags,
                request.Request.Version);

            _db.ChangelogEntries.Add(entry);
            await _db.SaveChangesAsync(cancellationToken);
            return entry.Id;
        }
    }
}
