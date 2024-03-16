using System.Reflection;
using Application.Services.System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Constants.GameServer;
using Domain.Contracts;
using Domain.Converters;
using MemoryPack;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Services.System;

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

    public byte[] SerializeMemory<T>(T rawObject)
    {
        return MemoryPackSerializer.Serialize(rawObject);
    }

    public T? DeserializeMemory<T>(byte[] rawMemory)
    {
        return MemoryPackSerializer.Deserialize<T>(rawMemory);
    }
}