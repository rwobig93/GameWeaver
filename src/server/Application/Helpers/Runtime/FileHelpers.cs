using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.Integrations;
using Domain.DatabaseEntities.Integrations;
using Domain.Enums.GameServer;
using Domain.Enums.Integrations;
using GameWeaverShared.Parsers;

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

    public static ContentType GetContentTypeFromName(string fileName)
    {
        return fileName switch
        {
            _ when fileName.EndsWith(".txt") => ContentType.Raw,
            _ when fileName.EndsWith(".ini") => ContentType.Ini,
            _ when fileName.EndsWith(".json") => ContentType.Json,
            _ when fileName.EndsWith(".xml") => ContentType.Xml,
            _ when fileName.EndsWith(".bat") => ContentType.Batch,
            _ when fileName.EndsWith(".lua") => ContentType.Lua,
            _ when fileName.EndsWith(".ps1") => ContentType.Powershell,
            _ when fileName.EndsWith(".py") => ContentType.Python,
            _ when fileName.EndsWith(".vb") => ContentType.VisualBasic,
            _ when fileName.EndsWith(".bin") => ContentType.Binary,
            _ when fileName.EndsWith(".exe") => ContentType.Binary,
            _ => ContentType.Raw
        };
    }

    public static FileEditorLanguage GetLanguageFromName(string fileName)
    {
        return fileName switch
        {
            _ when fileName.EndsWith(".txt") => FileEditorLanguage.Plaintext,
            _ when fileName.EndsWith(".ini") => FileEditorLanguage.Ini,
            _ when fileName.EndsWith(".json") => FileEditorLanguage.Json,
            _ when fileName.EndsWith(".xml") => FileEditorLanguage.Xml,
            _ when fileName.EndsWith(".bat") => FileEditorLanguage.Batch,
            _ when fileName.EndsWith(".lua") => FileEditorLanguage.Lua,
            _ when fileName.EndsWith(".ps1") => FileEditorLanguage.Powershell,
            _ when fileName.EndsWith(".py") => FileEditorLanguage.Python,
            _ when fileName.EndsWith(".vb") => FileEditorLanguage.VisualBasic,
            _ => FileEditorLanguage.Plaintext
        };
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

    public static IniData ToIni(this IEnumerable<ConfigurationItemSlim> configItems, bool allowDuplicates = false)
    {
        var iniFile = new IniData(allowDuplicates: allowDuplicates);
        var configurationItemLocals = configItems.ToList();

        var itemsWithPaths = configurationItemLocals
            .Where(item => !string.IsNullOrWhiteSpace(item.Path))
            .GroupBy(item => new { item.Path, item.Category })
            .Select(g => new ConfigurationItemSlim
            {
                Path = g.Key.Path,
                Key = g.Key.Path,
                Value = string.Join(",", g.Select(item => $"{item.Key}={item.Value}")),
                Category = g.Key.Category
            })
            .ToList();
        var rootItems = configurationItemLocals.Where(x => string.IsNullOrWhiteSpace(x.Path));

        foreach (var config in itemsWithPaths)
        {
            iniFile.AddOrUpdateKey(config.Category, config.Key, $"({config.Value})");
        }

        foreach (var config in rootItems)
        {
            iniFile.AddOrUpdateKey(config.Category, config.Key, config.Value);
        }

        return iniFile;
    }

    public static Dictionary<string, string> ToJsonReady(this IEnumerable<ConfigurationItemSlim> configItems)
    {
        var jsonReadyContent = new Dictionary<string, string>();

        foreach (var configItem in configItems)
        {
            if (!jsonReadyContent.ContainsKey(configItem.Key))
            {
                jsonReadyContent.Add(configItem.Key, configItem.Value);
                continue;
            }

            _ = jsonReadyContent.TryGetValue(configItem.Key, out var keyValue);
            if (keyValue == configItem.Value)
            {
                continue;
            }

            jsonReadyContent[configItem.Key] = configItem.Value;
        }

        return jsonReadyContent;
    }

    public static Dictionary<string, string> AggregateJsonReadyFrom(this Dictionary<string, string> original, Dictionary<string, string> source)
    {
        foreach (var configItem in source)
        {
            if (!original.ContainsKey(configItem.Key))
            {
                original.Add(configItem.Key, configItem.Value);
                continue;
            }

            _ = original.TryGetValue(configItem.Key, out var keyValue);
            if (keyValue == configItem.Value)
            {
                continue;
            }

            original[configItem.Key] = configItem.Value;
        }

        return original;
    }

    public static XDocument? ToXml(this IEnumerable<ConfigurationItemSlim> configItems)
    {
        var resourceConfigItems = configItems.ToList();
        var rootElement = resourceConfigItems.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Path));
        if (rootElement is null)
        {
            return null;
        }

        var xmlDocument = new XDocument(new XElement(rootElement.Key));
        foreach (var configItem in resourceConfigItems)
        {
            var parentElements = configItem.Path.Split('/');
            var configItemAttributes = configItem.Category.Split(',')
                .Select(x => x.Split(':'))
                .Where(x => x.Length == 2)
                .Select(x => new XAttribute(x[0], x[1]));
            var currentElement = xmlDocument.Root;

            // Create parent elements if there are any and move to the direct
            if (!string.IsNullOrWhiteSpace(configItem.Path))
            {
                foreach (var element in parentElements)
                {
                    var nextElement = currentElement?.Element(element);
                    if (nextElement == null)
                    {
                        nextElement = new XElement(element);
                        currentElement?.Add(nextElement);
                    }
                    currentElement = nextElement;
                }
            }

            var existingElement = currentElement?.Elements(configItem.Key).FirstOrDefault();
            if (existingElement is null)
            {
                // Element doesn't exist, so we'll create a new one
                var newElement = new XElement(configItem.Key, configItem.Value);

                if (configItemAttributes.Any())
                {
                    newElement.Add(configItemAttributes);
                }

                currentElement?.Add(newElement);
                continue;
            }

            // Update the existing element's value and attributes
            existingElement.Value = configItem.Value;
            existingElement.RemoveAttributes();

            if (configItemAttributes.Any())
            {
                existingElement.Add(configItemAttributes);
            }
        }

        return xmlDocument;
    }

    public static List<string> ToRaw(this IEnumerable<ConfigurationItemSlim> configItems)
    {
        // Raw config item keys are expected to be 'Line#' and 'Line#.#' for long lines
        return configItems.OrderBy(x => x.Key).Select(x => x.Value).ToList();
    }

    public static void AggregateRawFrom(this List<string> original, IEnumerable<ConfigurationItemSlim> source)
    {
        var sourceConfigItems = source.ToList();
        var maxConfigItemLines = sourceConfigItems.Select(item => int.Parse(item.Key)).Max();

        // Fill any config item gaps between lines, if nothing exists in the file at that line we will add an empty line
        foreach (var line in Enumerable.Range(0, maxConfigItemLines))
        {
            if (sourceConfigItems.Any(x => x.Key == line.ToString()))
            {
                continue;
            }

            sourceConfigItems.Add(new ConfigurationItemSlim { Key = line.ToString(), Value = string.Empty });
        }

        foreach (var configItem in sourceConfigItems.OrderBy(x => x.Key))
        {
            var lineIndex = int.Parse(configItem.Key);
            if (original.ElementAtOrDefault(lineIndex) is not null && string.IsNullOrWhiteSpace(configItem.Value))
            {
                // Existing line number isn't empty and our config item line is, so we'll leave the line value as it is
                continue;
            }

            original[lineIndex] = configItem.Value;
        }
    }
}