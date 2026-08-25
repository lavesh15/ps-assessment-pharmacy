namespace Pharmacy.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string resource, Guid id)
        : base("not_found", $"{resource} '{id}' was not found.")
    {
    }
}
