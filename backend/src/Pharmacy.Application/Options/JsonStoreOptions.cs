namespace Pharmacy.Application.Options;

public sealed class JsonStoreOptions
{
    public const string SectionName = "JsonStore";

    public string DataDirectory { get; set; } = "data";
}
