using Pharmacy.Domain.Entities;

namespace Pharmacy.Domain.Repositories;

public interface IMedicineRepository
{
    Task<IReadOnlyList<Medicine>> GetAllAsync(string? search, CancellationToken cancellationToken);
    Task<Medicine?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Medicine> AddAsync(Medicine medicine, CancellationToken cancellationToken);
    Task<(Medicine Medicine, Sale Sale)> SellAsync(
        Guid id,
        int quantity,
        int expectedVersion,
        CancellationToken cancellationToken);
}
