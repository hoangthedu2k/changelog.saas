namespace ChangelogSaas.Application.Interfaces
{
    public interface IBackgroundJobService
    {
        void EnqueueNotifySubscribers(Guid entryId);
    }
}
