using Pharmacy.Domain.Entities;

namespace Pharmacy.Domain.Repositories;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken);
    Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken);
}
