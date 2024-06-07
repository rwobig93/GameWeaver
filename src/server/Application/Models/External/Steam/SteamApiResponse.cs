using System.Text.Json;

namespace Application.Models.External.Steam;

public class SteamApiResponse
{
    public JsonElement Data { get; set; }
    public bool Success { get; set; }
}
