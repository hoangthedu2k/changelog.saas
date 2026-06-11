using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using ChangelogSaas.Domain.Plans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangelogSaas.Application.Entries.Commands.CreateEntryCommand
{
    public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, Guid>
    {
        private readonly IAppDbContext _db;

        public CreateEntryCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == request.Request.ProjectId, cancellationToken);
            if (project is null)
                throw new NotFoundException(nameof(Project), request.Request.ProjectId);

            var user = await _db.Users.FindAsync(new object[] { project.UserId }, cancellationToken);
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == project.UserId, cancellationToken);
            var plan = subscription?.Plan ?? SubscriptionPlan.Free;
            var inTrial = user?.IsInTrial ?? false;

            var entryCount = await _db.ChangelogEntries.CountAsync(e => e.ProjectId == request.Request.ProjectId, cancellationToken);
            if (entryCount >= PlanLimits.MaxEntries(plan, inTrial))
                throw new PlanLimitException($"Your {plan} plan allows up to {PlanLimits.MaxEntries(plan, inTrial)} entries per project. Upgrade to create more.");

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
