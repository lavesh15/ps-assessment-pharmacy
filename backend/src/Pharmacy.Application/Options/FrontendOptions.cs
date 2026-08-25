namespace Pharmacy.Application.Options;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string[] AllowedOrigins { get; set; } = [];
    public string CsrfCookieName { get; set; } = "pharmacy.csrf";
    public string CsrfHeaderName { get; set; } = "X-CSRF-TOKEN";
}
