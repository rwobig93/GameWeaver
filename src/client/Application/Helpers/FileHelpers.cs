using System.Security.Cryptography;

namespace Application.Helpers;

public static class FileHelpers
{
    public static string? ComputeSha256Hash(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        using (var sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                var normalizedHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return normalizedHash;
            }
        }
    }
}