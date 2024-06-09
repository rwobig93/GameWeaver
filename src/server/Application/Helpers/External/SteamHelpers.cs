using System.Text.Json;
using Application.Models.External.Steam;
using Application.Services.System;

namespace Application.Helpers.External;

public static class SteamHelpers
{
    public static SteamAppDetailResponseJson? ParseSteamAppDetailJson(this ISerializerService serializer, string jsonResponse)
    {
        using (var doc = JsonDocument.Parse(jsonResponse))
        {
            var root = doc.RootElement;

            var dynamicValueElement = root.EnumerateObject().First().Value;

            if (!dynamicValueElement.TryGetProperty("data", out var dataElement)) return null;
            
            var dataJson = dataElement.GetRawText();
            // TODO: dataJson is expected JSON formatted text but parsedResponse isn't getting values from any properties
            var parsedResponse = serializer.DeserializeJson<SteamAppDetailResponseJson>(dataJson);
            return parsedResponse;
        }
    }
}