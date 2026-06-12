using ChangelogSaas.Application.Interfaces;
using ChangelogSaas.Domain.Entities;
using ChangelogSaas.Domain.Enums;
using ChangelogSaas.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ChangelogSaas.Application.Subscribers.Commands.SubscribeCommand
{
    public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, string>
    {
        private readonly IAppDbContext _db;
        private readonly IEmailService _email;
        private readonly string _publicUrl;

        public SubscribeCommandHandler(IAppDbContext db, IEmailService email, IConfiguration config)
        {
            _db = db;
            _email = email;
            _publicUrl = config["App:PublicUrl"] ?? "http://localhost:4200";
        }

        public async Task<string> Handle(SubscribeCommand request, CancellationToken cancellationToken)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
                ?? throw new NotFoundException(nameof(Project), request.ProjectId);

            var email = request.Email.Trim().ToLowerInvariant();
            var existing = await _db.Subscribers
                .FirstOrDefaultAsync(s => s.ProjectId == request.ProjectId && s.Email == email, cancellationToken);

            string confirmToken;
            if (existing is not null)
            {
                if (existing.Status == SubscriberStatus.Verified)
                    throw new DomainException("Email is already subscribed.");
                confirmToken = existing.ConfirmToken!;
            }
            else
            {
                var subscriber = Subscriber.Create(request.ProjectId, email);
                _db.Subscribers.Add(subscriber);
                await _db.SaveChangesAsync(cancellationToken);
                confirmToken = subscriber.ConfirmToken!;
            }

            var confirmUrl = $"{_publicUrl}/confirm?token={confirmToken}";
            await _email.SendConfirmationEmailAsync(email, confirmUrl);

            return confirmToken;
        }
    }
}
