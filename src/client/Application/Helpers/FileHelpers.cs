using System.Security.Cryptography;
using System.Text;

namespace Application.Helpers;

public static class FileHelpers
{
    public static string GetIntegrityHash(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexStringLower(hash);
    }

    public static string GetIntegrityHash(Stream stream)
    {
        var hash = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hash);
    }

    public static string? ComputeFileContentSha256Hash(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        using (var stream = File.OpenRead(filePath))
        {
            return GetIntegrityHash(stream);
        }
    }

    public static string SanitizeSecureFilename(string filename)
    {
        if (filename.StartsWith(':'))
        {
            filename = filename[1..];
        }

        return filename.Replace("\\", "/").Replace("\"", "").Replace("'", "");
    }
}