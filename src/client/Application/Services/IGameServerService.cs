using Domain.Contracts;
using Domain.Models.GameServer;

namespace Application.Services;

public interface IGameServerService
{
    public static bool SteamCmdUpdateInProgress { get; private set; }
    Task<IResult> ValidateSteamCmdInstall();
    void ClearSteamCmdData();
    Task<IResult<SoftwareUpdateStatus>> CheckForUpdateGame();
    Task<IResult<SoftwareUpdateStatus>> CheckForUpdateMod();
    Task<IResult> InstallOrUpdateGame();
    Task<IResult> InstallOrUpdateMod();
    Task<IResult> UninstallGame();
    Task<IResult> UninstallMod();
}