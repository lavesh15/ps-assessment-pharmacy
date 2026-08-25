namespace Pharmacy.Application.DTOs;

public sealed record CreateMedicineRequest(
    string FullName,
    string? Notes,
    DateOnly ExpiryDate,
    int Quantity,
    decimal Price,
    string Brand);
