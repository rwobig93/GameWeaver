using System.Security.Cryptography;
using System.Text;
using Application.Models.Integrations;
using Domain.DatabaseEntities.Integrations;
using Domain.Enums.GameServer;

namespace Application.Helpers.Runtime;

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

    public static string GetServerRootDirectory()
    {
        return Path.Join(Directory.GetCurrentDirectory(), "_local_storage");
    }

    public static string GetLocalFilePath(this FileStorageRecordDb record)
    {
        return Path.Join(GetServerRootDirectory(), record.LinkedType.ToString(), record.LinkedId.ToString(), record.Id.ToString());
    }

    public static string GetLocalFilePath(this FileStorageRecordSlim record)
    {
        return Path.Join(GetServerRootDirectory(), record.LinkedType.ToString(), record.LinkedId.ToString(), record.Id.ToString());
    }

    public static string SanitizeSecureFilename(string filename)
    {
        if (filename.StartsWith(':'))
        {
            filename = filename[1..];
        }

        return filename.Replace("\\", "/").Replace("\"", "").Replace("'", "");
    }

    public static string GetFileExtension(this ContentType contentType)
    {
        return contentType switch
        {
            ContentType.Raw => "txt",
            ContentType.Ini => "ini",
            ContentType.Json => "json",
            ContentType.Xml => "xml",
            ContentType.Ignore => "",
            ContentType.Batch => "bat",
            ContentType.Lua => "lua",
            ContentType.Powershell => "ps1",
            ContentType.Python => "py",
            ContentType.VisualBasic => "vb",
            _ => ""
        };
    }
}