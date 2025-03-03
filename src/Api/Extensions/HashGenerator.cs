using System.Security.Cryptography;
using System.Text;

namespace Api.Extensions;

public static class HashGenerator
{
    public static string ComputeSha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}