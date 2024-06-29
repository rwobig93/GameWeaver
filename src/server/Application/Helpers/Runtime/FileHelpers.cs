using System.Security.Cryptography;
using Application.Models.System;
using Domain.DatabaseEntities.Integrations;

namespace Application.Helpers.Runtime;

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

    public static string GetServerRootDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    public static string GetLocalFilePath(this FileStorageRecordDb record)
    {
        return Path.Join(GetServerRootDirectory(), record.LinkedType.ToString(), record.LinkedId.ToString(), record.Id.ToString());
    }

    public static string GetLocalFilePath(this FileStorageRecordSlim record)
    {
        return Path.Join(GetServerRootDirectory(), record.LinkedType.ToString(), record.LinkedId.ToString(), record.Id.ToString());
    }
}