namespace Pharmacy.Application.DTOs;

public sealed record MedicineDetailDto(
    Guid Id,
    string FullName,
    string Notes,
    DateOnly ExpiryDate,
    int Quantity,
    decimal Price,
    string Brand,
    int Version);
