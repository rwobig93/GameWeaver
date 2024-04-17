using Domain.Contracts;
using Domain.Models.GameServer;

namespace Application.Services;

public interface IGameServerService
{
    public static bool SteamCmdUpdateInProgress { get; private set; }
    Task<IResult> ValidateSteamCmdInstall();
    void ClearSteamCmdData();
    Task<IResult<SoftwareUpdateStatus>> CheckForUpdateGame(GameServerLocal gameServer);
    Task<IResult<SoftwareUpdateStatus>> CheckForUpdateMod(GameServerLocal gameServer, Mod mod);
    Task<IResult> InstallOrUpdateGame(GameServerLocal gameServer);
    Task<IResult> InstallOrUpdateMod(GameServerLocal gameServer, Mod mod);
    Task<IResult> UninstallGame(GameServerLocal gameServer);
    Task<IResult> UninstallMod(GameServerLocal gameServer, Mod mod);
    Task<IResult> BackupGame(GameServerLocal gameServer);
    Task<IResult> StartServer(GameServerLocal gameServer);
    Task<IResult> StopServer(GameServerLocal gameServer);
}