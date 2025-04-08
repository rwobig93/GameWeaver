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
                Key = lineNumber.ToString(),
                Value = line,
                FriendlyName = $"Line{lineNumber}"
            });
        }

        return configItems;
    }

    public static List<ConfigurationItemSlim> MergeConfiguration(this IEnumerable<ConfigurationItemSlim> existing, IEnumerable<ConfigurationItemSlim> priorityItems,
        bool keepExistingIds = false)
    {
        var updatedConfiguration = existing.ToList();

        foreach (var priorityConfig in priorityItems)
        {
            if (priorityConfig.DuplicateKey)
            {
                var matchingConfigDuplicate = updatedConfiguration.FirstOrDefault(x =>
                    x.Category == priorityConfig.Category &&
                    x.Path == priorityConfig.Path &&
                    x.Key == priorityConfig.Key &&
                    x.Value == priorityConfig.Value);

                if (priorityConfig.Ignore && matchingConfigDuplicate is not null)
                {
                    updatedConfiguration.Remove(matchingConfigDuplicate);
                    continue;
                }

                if (matchingConfigDuplicate is not null)
                {
                    continue;
                }

                updatedConfiguration.Add(priorityConfig);
                continue;
            }

            // Key is not a duplicate key
            var matchingConfig = updatedConfiguration.FirstOrDefault(x =>
                x.Category == priorityConfig.Category &&
                x.Path == priorityConfig.Path &&
                x.Key == priorityConfig.Key);

            if (priorityConfig.Ignore && matchingConfig is not null)
            {
                updatedConfiguration.Remove(matchingConfig);
                continue;
            }

            if (matchingConfig is not null)
            {
                matchingConfig.Id = keepExistingIds ? matchingConfig.Id : priorityConfig.Id;
                matchingConfig.LocalResourceId = priorityConfig.LocalResourceId;
                matchingConfig.Value = priorityConfig.Value;
                continue;
            }

            updatedConfiguration.Add(priorityConfig);
        }

        return updatedConfiguration;
    }

    public static void UpdateEditorConfigFromExisting(this IEnumerable<ConfigurationItemSlim> sourceItems, IEnumerable<ConfigurationItemSlim> updateFrom)
    {
        var updateFromItems = updateFrom.ToList();
        foreach (var sourceItem in sourceItems)
        {
            if (sourceItem.DuplicateKey)
            {
                var matchingConfigDuplicate = updateFromItems.FirstOrDefault(x =>
                    x.Category == sourceItem.Category &&
                    x.Path == sourceItem.Path &&
                    x.Key == sourceItem.Key &&
                    x.Value == sourceItem.Value);

                if (matchingConfigDuplicate is null)
                {
                    continue;
                }

                sourceItem.Id = matchingConfigDuplicate.Id;
                sourceItem.FriendlyName = matchingConfigDuplicate.FriendlyName;
                sourceItem.Ignore = matchingConfigDuplicate.Ignore;
                continue;
            }

            // Key is not a duplicate key
            var matchingConfig = updateFromItems.FirstOrDefault(x =>
                x.Category == sourceItem.Category &&
                x.Path == sourceItem.Path &&
                x.Key == sourceItem.Key);

            if (matchingConfig is null)
            {
                continue;
            }

            sourceItem.Id = matchingConfig.Id;
            sourceItem.FriendlyName = matchingConfig.FriendlyName;
            sourceItem.Ignore = matchingConfig.Ignore;
        }
    }
}