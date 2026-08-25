namespace Pharmacy.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(int requested, int available)
        : base("insufficient_stock", $"Cannot sell {requested} unit(s); only {available} in stock.")
    {
    }
}
