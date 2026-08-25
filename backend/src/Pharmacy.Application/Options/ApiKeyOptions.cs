namespace Pharmacy.Application.Options;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";

    public string Value { get; set; } = string.Empty;
    public string CookieName { get; set; } = "pharmacy.apikey";
}
