using ChangelogSaas.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogSaas.Domain.Entities
{
    public class EmailLogs
    {
        public Guid Id { get; private set; }
        public Guid EntryID { get; private set; }
        public Guid SubscriberID { get; private set; }

        public EmailStatus Status { get; private set; }

        public string? ResendMessageID { get; private set; }
        public DateTime? OpenedAt { get; private set; }

        public DateTime SendAt { get; private set; }

    }
}
