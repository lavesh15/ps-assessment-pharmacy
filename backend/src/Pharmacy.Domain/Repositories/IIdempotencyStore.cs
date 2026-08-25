using Pharmacy.Domain.Entities;

namespace Pharmacy.Domain.Repositories;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken);
    Task SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken);
}
