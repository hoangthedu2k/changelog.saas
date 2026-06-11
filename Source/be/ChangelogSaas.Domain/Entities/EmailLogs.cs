using ChangelogSaas.Domain.Enums;

namespace ChangelogSaas.Domain.Entities
{
    public class EmailLogs : BaseEntity
    {
        public Guid EntryID { get; private set; }
        public Guid SubscriberID { get; private set; }
        public EmailStatus Status { get; private set; }
        public string? ResendMessageID { get; private set; }
        public DateTime? OpenedAt { get; private set; }

        public static EmailLogs Create(Guid entryId, Guid subscriberId)
        {
            return new EmailLogs
            {
                Id = Guid.NewGuid(),
                EntryID = entryId,
                SubscriberID = subscriberId,
                Status = EmailStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkSent(string resendMessageId)
        {
            Status = EmailStatus.Sent;
            ResendMessageID = resendMessageId;
        }

        public void MarkOpened()
        {
            OpenedAt = DateTime.UtcNow;
        }
    }
}
