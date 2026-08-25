namespace Pharmacy.Api;

internal static class AppHeaders
{
    public const string CorrelationId = "X-Correlation-ID";
    public const string IdempotencyKey = "Idempotency-Key";
    public const string CsrfToken = "X-CSRF-TOKEN";
}
