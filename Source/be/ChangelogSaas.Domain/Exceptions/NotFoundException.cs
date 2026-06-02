namespace ChangelogSaas.Domain.Exceptions
{
    public class NotFoundException(string entity, object key)
        : DomainException($"{entity} with id '{key}' was not found.")
    { }
}
