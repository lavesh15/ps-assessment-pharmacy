namespace Pharmacy.Domain.Exceptions;

public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException()
        : base("concurrency_conflict", "The medicine was updated by another request. Refresh and try again.")
    {
    }
}
