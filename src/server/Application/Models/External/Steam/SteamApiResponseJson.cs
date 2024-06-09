using System.Text.Json;

namespace Application.Models.External.Steam;

public class SteamApiResponseJson
{
    public JsonElement Data { get; set; }
    public bool Success { get; set; }
}
