namespace GameServerQuery.Steam.Models;

public class SteamServerRulesInfo
{
    public int RuleCount { get; set; }
    public List<SteamServerRule> Rules { get; set; } = [];
}