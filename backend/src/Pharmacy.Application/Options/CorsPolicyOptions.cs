namespace Pharmacy.Application.Options;

public sealed class CorsPolicyOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
