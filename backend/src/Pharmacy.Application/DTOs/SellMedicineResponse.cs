namespace Pharmacy.Application.DTOs;

public sealed record SellMedicineResponse(
    Guid Id,
    Guid MedicineId,
    int Quantity,
    DateTime SoldAtUtc,
    int RemainingQuantity,
    int Version);
