﻿using System.Text;
using GameWeaverShared.Parsers.Models;

namespace GameWeaverShared.Parsers;

public class IniData
{
    public List<IniSection> Sections { get; } = [];
    private readonly bool _allowDuplicates;

    public IniData(string? filePath = null, bool allowDuplicates = false)
    {
        _allowDuplicates = allowDuplicates;
        if (filePath is not null)
        {
            Load(filePath, true);
        }
    }

    public IniData(IEnumerable<string> fileContent, bool allowDuplicates = false)
    {
        _allowDuplicates = allowDuplicates;
        Load(fileContent, true);
    }

    public void Load(string filePath, bool loadClean = false)
    {
        if (loadClean)
        {
            Sections.Clear();
        }

        if (!File.Exists(filePath))
        {
            return;
        }

        var lines = File.ReadAllLines(filePath);
        Load(lines, loadClean);
    }

    public void Load(IEnumerable<string> content, bool loadClean = false)
    {
        if (loadClean)
        {
            Sections.Clear();
        }

        IniSection? currentSection = null;

        foreach (var line in content)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith($";"))
            {
                continue; // Skip comments and empty lines
            }

            if (trimmedLine.StartsWith($"[") && trimmedLine.EndsWith($"]"))
            {
                var sectionName = trimmedLine[1..^1];
                currentSection = AddSection(sectionName);
            }

            if (currentSection == null)
            {
                continue;
            }

            var keyAndValue = trimmedLine.Split(['='], 2);
            if (keyAndValue.Length != 2)
            {
                continue;
            }

            var key = keyAndValue[0].Trim();
            var value = keyAndValue[1].Trim();
            AddOrUpdateKey(currentSection.Name, key, value);
        }
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        var lastSection = Sections.LastOrDefault();
        foreach (var section in Sections)
        {
            builder.Append($"[{section.Name}]{Environment.NewLine}");
            foreach (var keyValuePair in section.Keys)
            {
                builder.Append($"{keyValuePair.Key}={keyValuePair.Value}{Environment.NewLine}");
            }

            if (lastSection is not null && section != lastSection)  // Skip the last section newline so there aren't two line breaks
            {
                builder.Append(Environment.NewLine); // New line for readability between Sections
            }
        }

        return builder.ToString();
    }

    public async Task<ParserResult> Save(string filePath)
    {
        try
        {
            await using var file = new StreamWriter(filePath);
            foreach (var section in Sections)
            {
                file.WriteLine($"[{section.Name}]");
                foreach (var keyValuePair in section.Keys)
                {
                    file.WriteLine($"{keyValuePair.Key}={keyValuePair.Value}");
                }

                file.WriteLine(); // New line for readability between Sections
            }

            return await Task.FromResult(new ParserResult {Succeeded = true});
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new ParserResult {Succeeded = false, Messages = [ex.Message]});
        }
    }

    public IniSection AddSection(string sectionName)
    {
        var matchingSection = Sections.FirstOrDefault(x => x.Name == sectionName);
        if (matchingSection is not null)
        {
            return matchingSection;
        }

        var newSection = new IniSection {Name = sectionName};
        Sections.Add(newSection);
        return Sections.First(x => x.Name == newSection.Name);
    }

    public void RemoveSection(string sectionName)
    {
        var matchingSections = Sections.Where(x => x.Name == sectionName).ToList();
        if (matchingSections.Count == 0)
        {
            return;
        }

        foreach (var section in matchingSections)
        {
            Sections.Remove(section);
        }
    }

    public void AddOrUpdateKey(string sectionName, string key, string value)
    {
        var matchingSections = Sections.Where(x => x.Name == sectionName).ToList();
        if (matchingSections.Count == 0)
        {
            var newSection = AddSection(sectionName);
            matchingSections.Add(newSection);
        }

        var selectedSection = matchingSections.First();
        var matchingKeys = selectedSection.Keys.Where(x => x.Key == key).ToList();
        if (matchingKeys.Count == 0)
        {
            selectedSection.Keys.Add(new IniKeyValue {Key = key, Value = value});
            return;
        }

        if (_allowDuplicates)
        {
            var matchingKeyValue = matchingKeys.FirstOrDefault(x => x.Value == value);
            if (matchingKeyValue is not null)
            {
                return;
            }

            selectedSection.Keys.Add(new IniKeyValue {Key = key, Value = value});
            return;
        }

        var matchingKey = matchingKeys.First();
        if (matchingKey.Value == value)
        {
            return;
        }

        matchingKey.Value = value;
    }

    public void RemoveKey(string sectionName, string key, string? value = null)
    {
        var matchingSections = Sections.Where(x => x.Name == sectionName).ToList();
        if (matchingSections.Count == 0)
        {
            return;
        }

        var selectedSection = matchingSections.First();
        var matchingKeys = value switch
        {
            not null => selectedSection.Keys.Where(x => x.Key == key && x.Value == value).ToList(),
            _ => selectedSection.Keys.Where(x => x.Key == key).ToList()
        };
        if (matchingKeys.Count == 0)
        {
            return;
        }

        foreach (var matchingKey in matchingKeys)
        {
            selectedSection.Keys.Remove(matchingKey);
        }
    }

    public void AggregateFrom(IniData source)
    {
        foreach (var section in source.Sections)
        {
            foreach (var keyValuePair in section.Keys)
            {
                AddOrUpdateKey(section.Name, keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}