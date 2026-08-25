namespace Pharmacy.Domain.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("invalid_credentials", "Invalid username or password.")
    {
    }
}
