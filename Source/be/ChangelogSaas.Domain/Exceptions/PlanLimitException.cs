namespace ChangelogSaas.Domain.Exceptions
{
    public class PlanLimitException(string message) : DomainException(message) { }
}
