namespace Pharmacy.Application.Options;

public sealed class DemoAuthOptions
{
    public const string SectionName = "DemoAuth";

    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "Admin@123";
}
