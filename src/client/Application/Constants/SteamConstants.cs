using Domain.Models.GameServer;

namespace Application.Constants;

public class SteamConstants
{
    public const string SteamCmdDownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

    public const string CommandUpdate = "update +quit";

    public static string CommandInstallUpdateGame(GameServerLocal gameServer)
    {
        return $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{gameServer.InstallDirectory}\" +login anonymous" +
               $" +app_info_update 1 +app_update \"{gameServer.SteamToolId}\" +app_status {gameServer.SteamToolId} +quit";
    }
}