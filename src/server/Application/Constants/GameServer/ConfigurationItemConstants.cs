using Application.Models.GameServer.ConfigurationItem;

namespace Application.Constants.GameServer;

public static class ConfigurationItemConstants
{
    public static readonly List<ConfigurationDynamicVariable> DynamicVariables =
    [
        new() {Variable = "%%%SERVER_NAME%%%", FriendlyName = "Server Name"},
        new() {Variable = "%%%PASSWORD%%%", FriendlyName = "Server Password"},
        new() {Variable = "%%%QUERY_PORT%%%", FriendlyName = "Query Port"},
        new() {Variable = "%%%GAME_PORT%%%", FriendlyName = "Game Port"},
        new() {Variable = "%%%GAME_PORT_PEER%%%", FriendlyName = "Game Peer Port"},
        new() {Variable = "%%%PASSWORD_ADMIN%%%", FriendlyName = "Admin Password"},
        new() {Variable = "%%%PASSWORD_RCON%%%", FriendlyName = "RCON Password"},
    ];
}