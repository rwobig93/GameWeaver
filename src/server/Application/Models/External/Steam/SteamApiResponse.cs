using System.Text.Json;

namespace Application.Models.External.Steam;

public class SteamApiResponse
{
    public JsonElement data { get; set; }
    public bool success { get; set; }
}
