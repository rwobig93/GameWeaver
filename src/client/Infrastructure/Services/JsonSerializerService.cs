using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Constants;
using Application.Services;
using Domain.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure.Services;

public class JsonSerializerService : ISerializerService
{
    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.Strict,
        IgnoreReadOnlyProperties = false,
        IgnoreReadOnlyFields = false,
        IncludeFields = false,
        MaxDepth = 64,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        WriteIndented = true
    };

    public string Serialize<T>(T rawObject)
    {
        return JsonSerializer.Serialize(rawObject, _options);
    }

    public T Deserialize<T>(string rawJson)
    {
        return JsonSerializer.Deserialize<T>(rawJson, _options) ?? throw new InvalidOperationException();
    }

    public T Deserialize<T>(byte[] rawJson)
    {
        return JsonSerializer.Deserialize<T>(rawJson, _options) ?? throw new InvalidOperationException();
    }

    public async Task<IResult> SaveSettings(string settingsSectionName, object updatedSection)
    {
        try
        {
            // Load the file
            var filePath = Path.Combine(AppContext.BaseDirectory, HostConstants.ConfigFile);
            var json = File.ReadAllText(filePath);
            var jsonObj = JsonSerializer.Deserialize<dynamic>(json)!;

            // Set the new value
            JContainer section = jsonObj[settingsSectionName];
            if (section is not null)
            {
                foreach (var prop in updatedSection.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    section[prop.Name] = new JValue(prop.GetValue(updatedSection));
                }
            }

            // Save the file
            string output = JsonSerializer.Serialize(jsonObj, Formatting.Indented);
            File.WriteAllText(filePath, output);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
}