using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pharmacy.Application.Options;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Exceptions;
using Pharmacy.Domain.Repositories;

namespace Pharmacy.Infrastructure.Persistence;

internal sealed class JsonPharmacyStore : IMedicineRepository, ISaleRepository, IIdempotencyStore
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly JsonFileStore<Medicine> _medicines;
    private readonly JsonFileStore<Sale> _sales;
    private readonly JsonFileStore<IdempotencyRecord> _idempotency;

    public JsonPharmacyStore(IOptions<JsonStoreOptions> options, IHostEnvironment environment)
    {
        var configured = options.Value.DataDirectory;
        var dataDirectory = Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configured));

        Directory.CreateDirectory(dataDirectory);

        _medicines = new JsonFileStore<Medicine>(Path.Combine(dataDirectory, "medicines.json"), Gate);
        _sales = new JsonFileStore<Sale>(Path.Combine(dataDirectory, "sales.json"), Gate);
        _idempotency = new JsonFileStore<IdempotencyRecord>(Path.Combine(dataDirectory, "idempotency.json"), Gate);
    }

    public Task<IReadOnlyList<Medicine>> GetAllAsync(string? search, CancellationToken cancellationToken)
    {
        return _medicines.LockedAsync(async () =>
        {
            var items = await _medicines.ReadAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items
                    .Where(m => m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return (IReadOnlyList<Medicine>)items
                .OrderBy(m => m.FullName)
                .ToList();
        }, cancellationToken);
    }

    public Task<Medicine?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _medicines.LockedAsync(async () =>
        {
            var items = await _medicines.ReadAsync(cancellationToken);
            return items.FirstOrDefault(m => m.Id == id);
        }, cancellationToken);
    }

    public Task<Medicine> AddAsync(Medicine medicine, CancellationToken cancellationToken)
    {
        return _medicines.MutateAsync(items =>
        {
            items.Add(medicine);
            return medicine;
        }, cancellationToken);
    }

    public Task<(Medicine Medicine, Sale Sale)> SellAsync(
        Guid id,
        int quantity,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        return _medicines.LockedAsync(async () =>
        {
            var medicines = await _medicines.ReadAsync(cancellationToken);
            var medicine = medicines.FirstOrDefault(m => m.Id == id)
                ?? throw new NotFoundException(nameof(Medicine), id);

            if (medicine.Version != expectedVersion)
            {
                throw new ConcurrencyException();
            }

            if (quantity > medicine.Quantity)
            {
                throw new InsufficientStockException(quantity, medicine.Quantity);
            }

            medicine.Quantity -= quantity;
            medicine.Version++;

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                MedicineId = medicine.Id,
                Quantity = quantity,
                SoldAtUtc = DateTime.UtcNow
            };

            var sales = await _sales.ReadAsync(cancellationToken);
            sales.Add(sale);

            await _medicines.WriteAsync(medicines, cancellationToken);
            await _sales.WriteAsync(sales, cancellationToken);

            return (medicine, sale);
        }, cancellationToken);
    }

    public Task AddAsync(Sale sale, CancellationToken cancellationToken)
    {
        return _sales.MutateAsync(items =>
        {
            items.Add(sale);
            return sale;
        }, cancellationToken);
    }

    public Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _sales.LockedAsync(async () =>
        {
            var items = await _sales.ReadAsync(cancellationToken);
            return (IReadOnlyList<Sale>)items.OrderByDescending(s => s.SoldAtUtc).ToList();
        }, cancellationToken);
    }

    public Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken)
    {
        return _idempotency.LockedAsync(async () =>
        {
            var items = await _idempotency.ReadAsync(cancellationToken);
            var cutoff = DateTime.UtcNow.AddHours(-24);
            return items.FirstOrDefault(r => r.Key == key && r.CreatedAtUtc >= cutoff);
        }, cancellationToken);
    }

    public Task SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken)
    {
        return _idempotency.MutateAsync(items =>
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            items.RemoveAll(r => r.CreatedAtUtc < cutoff || r.Key == record.Key);
            items.Add(record);
            return record;
        }, cancellationToken);
    }
}
