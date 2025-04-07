using System.Xml.Linq;
using Application.Models.GameServer.ConfigurationItem;
using GameWeaverShared.Parsers;

namespace Application.Helpers.GameServer;

public static class ConfigurationItemHelpers
{
    public static List<ConfigurationItemSlim> ToConfigItems(this IniData iniData, Guid resourceId)
    {
        List<ConfigurationItemSlim> configItems = [];

        configItems.AddRange(from section in iniData.Sections from entry in section.Keys
        let isDuplicate = section.Keys.Count(x => x.Key == entry.Key) > 1
        select new ConfigurationItemSlim
        {
            Id = Guid.CreateVersion7(),
            LocalResourceId = resourceId,
            DuplicateKey = isDuplicate,
            Ignore = false,
            Path = string.Empty,
            Category = section.Name,
            Key = entry.Key,
            Value = entry.Value,
            FriendlyName = entry.Key
        });

        return configItems;
    }

    public static List<ConfigurationItemSlim> ToConfigItems(this Dictionary<string, string> jsonData, Guid resourceId)
    {
        List<ConfigurationItemSlim> configItems = [];

        configItems.AddRange(jsonData.Select(keyValuePair => new ConfigurationItemSlim
        {
            Id = Guid.CreateVersion7(),
            LocalResourceId = resourceId,
            DuplicateKey = false,
            Ignore = false,
            Path = string.Empty,
            Category = string.Empty,
            Key = keyValuePair.Key,
            Value = keyValuePair.Value,
            FriendlyName = keyValuePair.Key
        }));

        return configItems;
    }

    public static List<ConfigurationItemSlim> ToConfigItems(this XDocument xmlData, Guid resourceId)
    {
        List<ConfigurationItemSlim> configItems = [];

        configItems.AddRange(xmlData.Root!.Elements()
        .Select(element => new ConfigurationItemSlim
        {
            Id = Guid.CreateVersion7(),
            LocalResourceId = resourceId,
            DuplicateKey = false,
            Ignore = false,
            Path = string.Join("/", element.AncestorsAndSelf().Reverse().Select(a => a.Name.LocalName).ToArray()),
            Category = string.Join(",", element.Attributes()),
            Key = element.Name.LocalName,
            Value = element.Value,
            FriendlyName = element.Name.LocalName
        }));

        return configItems;
    }

    public static List<ConfigurationItemSlim> ToConfigItems(this IEnumerable<string> rawData, Guid resourceId)
    {
        List<ConfigurationItemSlim> configItems = [];

        var lineNumber = 0;
        foreach (var line in rawData)
        {
            lineNumber++;
            configItems.Add(new ConfigurationItemSlim
            {
                Id = Guid.CreateVersion7(),
                LocalResourceId = resourceId,
                DuplicateKey = false,
                Ignore = false,
                Path = string.Empty,
                Category = string.Empty,
                Key = $"Line{lineNumber}",
                Value = line,
                FriendlyName = $"Line{lineNumber}"
            });
        }

        return configItems;
    }
}