namespace Pharmacy.Domain.Entities;

public sealed class IdempotencyRecord
{
    public string Key { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string BodyJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
