using System.Security.Cryptography;

namespace Pharmacy.Api;

internal static class TokenFactory
{
    public static string Create() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
