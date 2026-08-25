namespace Pharmacy.Domain.Entities;

public sealed class Sale
{
    public Guid Id { get; set; }
    public Guid MedicineId { get; set; }
    public int Quantity { get; set; }
    public DateTime SoldAtUtc { get; set; }
}
