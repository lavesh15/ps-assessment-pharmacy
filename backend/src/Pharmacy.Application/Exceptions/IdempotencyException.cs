namespace Pharmacy.Application.Exceptions;

public sealed class IdempotencyException : Exception
{
    public IdempotencyException(string message) : base(message)
    {
    }
}
