namespace Pharmacy.Domain.Entities;

public sealed class Medicine
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
}
