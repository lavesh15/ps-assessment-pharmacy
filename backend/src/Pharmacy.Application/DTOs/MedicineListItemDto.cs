namespace Pharmacy.Application.DTOs;

public sealed record MedicineListItemDto(
    Guid Id,
    string FullName,
    DateOnly ExpiryDate,
    int Quantity,
    decimal Price,
    string Brand,
    int Version);
