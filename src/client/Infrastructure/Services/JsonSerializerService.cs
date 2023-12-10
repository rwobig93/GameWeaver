using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Services;

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
}