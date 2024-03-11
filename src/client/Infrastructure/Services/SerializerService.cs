using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Constants;
using Application.Helpers;
using Application.Services;
using Domain.Contracts;
using Domain.Converters;
using MemoryPack;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure.Services;

public class SerializerService : ISerializerService
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
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        WriteIndented = true,
        Converters = { new IpAddressConverter() }
    };

    public string SerializeJson<T>(T rawObject)
    {
        return JsonSerializer.Serialize(rawObject, _options);
    }

    public T DeserializeJson<T>(string rawJson)
    {
        return JsonSerializer.Deserialize<T>(rawJson, _options) ?? throw new InvalidOperationException();
    }

    public T DeserializeJson<T>(byte[] rawJson)
    {
        return JsonSerializer.Deserialize<T>(rawJson, _options) ?? throw new InvalidOperationException();
    }

    public async Task<IResult> SaveSettings(string settingsSectionName, object updatedSection)
    {
        try
        {
            // Load the file
            #if DEBUG
                var filePath = Path.Combine(OsHelper.GetDebugSanitizedPath(AppContext.BaseDirectory), HostConstants.ConfigFile);
            #else
                var filePath = Path.Combine(AppContext.BaseDirectory, HostConstants.ConfigFile);
            #endif
            var json = File.ReadAllText(filePath);
            var jsonObj = JObject.Parse(json);

            // Set the new value
            var section = jsonObj[settingsSectionName];
            if (section is JContainer jContainer)
            {
                foreach (var prop in updatedSection.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    jContainer[prop.Name] = JToken.FromObject(prop.GetValue(updatedSection)!);
                }
            }
            
            // Save the file
            var exportFile = jsonObj.ToString();
            File.WriteAllText(filePath, exportFile);

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    public byte[] SerializeMemory<T>(T rawObject)
    {
        return MemoryPackSerializer.Serialize(rawObject);
    }

    public T? DeserializeMemory<T>(byte[] rawMemory)
    {
        return MemoryPackSerializer.Deserialize<T>(rawMemory);
    }
}