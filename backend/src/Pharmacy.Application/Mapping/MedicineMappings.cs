using Pharmacy.Application.DTOs;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Mapping;

public static class MedicineMappings
{
    public static MedicineListItemDto ToListItem(this Medicine medicine) =>
        new(
            medicine.Id,
            medicine.FullName,
            medicine.ExpiryDate,
            medicine.Quantity,
            decimal.Round(medicine.Price, 2),
            medicine.Brand,
            medicine.Version);

    public static MedicineDetailDto ToDetail(this Medicine medicine) =>
        new(
            medicine.Id,
            medicine.FullName,
            medicine.Notes,
            medicine.ExpiryDate,
            medicine.Quantity,
            decimal.Round(medicine.Price, 2),
            medicine.Brand,
            medicine.Version);
}
