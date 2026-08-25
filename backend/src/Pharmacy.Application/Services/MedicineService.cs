using FluentValidation;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Exceptions;
using Pharmacy.Application.Mapping;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Exceptions;
using Pharmacy.Domain.Repositories;

namespace Pharmacy.Application.Services;

public interface IMedicineService
{
    Task<IReadOnlyList<MedicineListItemDto>> ListAsync(string? search, CancellationToken cancellationToken);
    Task<MedicineDetailDto> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<MedicineDetailDto> CreateAsync(CreateMedicineRequest request, string? idempotencyKey, CancellationToken cancellationToken);
    Task<SellMedicineResponse> SellAsync(Guid id, SellMedicineRequest request, string? idempotencyKey, CancellationToken cancellationToken);
}

public sealed class MedicineService : IMedicineService
{
    private readonly IMedicineRepository _medicines;
    private readonly IIdempotencyStore _idempotency;
    private readonly IValidator<CreateMedicineRequest> _createValidator;
    private readonly IValidator<SellMedicineRequest> _sellValidator;

    public MedicineService(
        IMedicineRepository medicines,
        IIdempotencyStore idempotency,
        IValidator<CreateMedicineRequest> createValidator,
        IValidator<SellMedicineRequest> sellValidator)
    {
        _medicines = medicines;
        _idempotency = idempotency;
        _createValidator = createValidator;
        _sellValidator = sellValidator;
    }

    public async Task<IReadOnlyList<MedicineListItemDto>> ListAsync(string? search, CancellationToken cancellationToken)
    {
        var items = await _medicines.GetAllAsync(search, cancellationToken);
        return items.Select(m => m.ToListItem()).ToList();
    }

    public async Task<MedicineDetailDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var medicine = await _medicines.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Medicine), id);
        return medicine.ToDetail();
    }

    public async Task<MedicineDetailDto> CreateAsync(
        CreateMedicineRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);
        RequireIdempotencyKey(idempotencyKey);

        var existing = await _idempotency.GetAsync(idempotencyKey!, cancellationToken);
        if (existing is not null)
        {
            return DeserializeOrThrow<MedicineDetailDto>(existing.BodyJson);
        }

        var medicine = new Medicine
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            ExpiryDate = request.ExpiryDate,
            Quantity = request.Quantity,
            Price = decimal.Round(request.Price, 2),
            Brand = request.Brand.Trim(),
            Version = 1
        };

        var saved = await _medicines.AddAsync(medicine, cancellationToken);
        var dto = saved.ToDetail();

        await _idempotency.SaveAsync(new IdempotencyRecord
        {
            Key = idempotencyKey!,
            Method = "POST",
            Path = "/api/v1/medicines",
            StatusCode = 201,
            BodyJson = System.Text.Json.JsonSerializer.Serialize(dto),
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return dto;
    }

    public async Task<SellMedicineResponse> SellAsync(
        Guid id,
        SellMedicineRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_sellValidator, request, cancellationToken);
        RequireIdempotencyKey(idempotencyKey);

        var existing = await _idempotency.GetAsync(idempotencyKey!, cancellationToken);
        if (existing is not null)
        {
            return DeserializeOrThrow<SellMedicineResponse>(existing.BodyJson);
        }

        var (medicine, sale) = await _medicines.SellAsync(id, request.Quantity, request.Version, cancellationToken);
        var dto = new SellMedicineResponse(
            sale.Id,
            sale.MedicineId,
            sale.Quantity,
            sale.SoldAtUtc,
            medicine.Quantity,
            medicine.Version);

        await _idempotency.SaveAsync(new IdempotencyRecord
        {
            Key = idempotencyKey!,
            Method = "POST",
            Path = $"/api/v1/medicines/{id}/sell",
            StatusCode = 200,
            BodyJson = System.Text.Json.JsonSerializer.Serialize(dto),
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return dto;
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new RequestValidationException(errors);
        }
    }

    private static void RequireIdempotencyKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new IdempotencyException("Idempotency-Key header is required for this request.");
        }
    }

    private static T DeserializeOrThrow<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json)
            ?? throw new IdempotencyException("Stored idempotent response could not be replayed.");
    }
}
