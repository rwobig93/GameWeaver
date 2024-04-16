using System.Diagnostics;
using System.IO.Compression;
using Application.Constants;
using Application.Helpers;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class GameServerService : IGameServerService
{
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;

    public GameServerService(ILogger logger, IDateTimeService dateTimeService, IOptions<GeneralConfiguration> generalConfig)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _generalConfig = generalConfig;
    }

    public static bool SteamCmdUpdateInProgress { get; private set; }

    public async Task<IResult> ValidateSteamCmdInstall()
    {
        var steamCmdDir = OsHelpers.GetSteamCmdDirectory();
        var steamCmdPath = OsHelpers.GetSteamCmdPath();
        var steamCmdDownloadPath = Path.Combine(OsHelpers.GetSteamCachePath(), "steamcmd.zip");
        
        if (!File.Exists(steamCmdPath))
        {
            SteamCmdUpdateInProgress = true;
            _logger.Information("SteamCMD doesn't exist, installing fresh");
            _logger.Debug("SteamCMD path: {File}", steamCmdPath);
            
            var downloadResult = await DownloadSteamCmd(steamCmdDownloadPath);
            if (!downloadResult.Succeeded)
            {
                SteamCmdUpdateInProgress = false;
                return downloadResult;
            }
            
            var extractResult = await ExtractSteamCmdArchive(steamCmdDownloadPath, steamCmdDir);
            if (!extractResult.Succeeded)
            {
                SteamCmdUpdateInProgress = false;
                return extractResult;
            }

            SteamCmdUpdateInProgress = false;
            _logger.Information("Successfully installed SteamCMD at {Path}", steamCmdPath);
        }
        
        _logger.Debug("SteamCMD is installed, checking for updates...");
        SteamCmdUpdateInProgress = true;
        var updateResult = await RunSteamCmdCommand(SteamConstants.CommandUpdate, true);
        if (!updateResult.Succeeded)
            return updateResult;

        return await Result.SuccessAsync();
    }

    public void ClearSteamCmdData()
    {
        SteamCmdUpdateInProgress = true;
        var steamCmdDir = new DirectoryInfo(OsHelpers.GetSteamCmdDirectory());
        _logger.Information("Clearing SteamCMD data at: {Directory}", steamCmdDir);
        
        foreach (var file in steamCmdDir.EnumerateFiles())
        {
            try
            {
                if (file.FullName == OsHelpers.GetSteamCmdPath())
                {
                    _logger.Verbose("Skipping steamcmd binary: {File}", file.FullName);
                    continue;
                }
                
                _logger.Debug("Deleting File: {File}", file.FullName);
                file.Delete();
            }
            catch (Exception ex)
            {
                _logger.Information(ex, "Failed to delete SteamCMD cache fiel: {Error}", ex.Message);
            }
        }
        _logger.Information("Finished deleting root SteamCMD data files");

        foreach (var dir in steamCmdDir.EnumerateDirectories())
        {
            try
            {
                _logger.Debug("Deleting directory recursively: {Directory}", dir.FullName);
                dir.Delete(true);
            }
            catch (Exception ex)
            {
                _logger.Information(ex, "Failed to delete SteamCMD cache directory: {Error}", ex.Message);
            }
        }
        _logger.Information("Finished deleting SteamCMD data directories");
        
        RunSteamCmdCommand(SteamConstants.CommandUpdate, true).RunSynchronously();
    }

    private async Task<IResult> DownloadSteamCmd(string downloadPath)
    {
        try
        {
            if (File.Exists(downloadPath))
            {
                _logger.Debug("Existing SteamCMD download exists, deleting before downloading: {FilePath}", downloadPath);
                File.Delete(downloadPath);
            }

            var downloadDirectory = new FileInfo(downloadPath).DirectoryName ?? OsHelpers.GetSteamCachePath();
            Directory.CreateDirectory(downloadDirectory);
            _logger.Debug("SteamCMD download directory: {Directory}", downloadDirectory);
            using (var httpClient = new HttpClient())
            {
                var responseStream = await httpClient.GetStreamAsync(SteamConstants.SteamCmdDownloadUrl);
                await using var fileStream = new FileStream(downloadPath, FileMode.Create);
                await responseStream.CopyToAsync(fileStream);
            }

            return await Result.SuccessAsync($"Successfully downloaded SteamCMD to: {downloadPath}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred downloading SteamCMD: {Error}", ex.Message);
            return await Result.FailAsync($"Failed to download SteamCMD: {ex.Message}");
        }
    }

    private async Task<IResult> ExtractSteamCmdArchive(string archivePath, string destinationPath)
    {
        try
        {
            Directory.CreateDirectory(destinationPath);
            _logger.Debug("Created SteamCMD directory for archive extraction: {Directory}", destinationPath);
            
            ZipFile.ExtractToDirectory(archivePath, destinationPath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to extract SteamCMD archive: {Error}", ex.Message);
            return await Result.FailAsync($"Failed to extract SteamCMD archive: {ex.Message}");
        }
    }

    private async Task<IResult> RunSteamCmdCommand(string command, bool isMaintenanceCommand = false)
    {
        // .\steamcmd.exe +login anonymous +force_install_dir "ConanExiles" +app_update 443030 validate +quit
        // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login anonymous +force_install_dir "ConanExiles" +app_info_update 1 +app_update 443030 +app_status 443030 +quit
        // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login ${SteamLoginName} +app_info_update 1 +app_status ${SteamAppID} +quit/
        // https://steamapi.xpaw.me/
        _logger.Debug("Running SteamCMD command: [Maintenance:{IsMaintenance}]{Command}", isMaintenanceCommand, command);
        try
        {
            var steamCmd = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = OsHelpers.GetSteamCmdPath(),
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            steamCmd.Start();

            HandleSteamCmdOutput(steamCmd, isMaintenanceCommand);
            _logger.Debug("Finished running SteamCMD command: {Command}", command);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred running SteamCMD command: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred running SteamCMD command: {ex.Message}");
        }
    }

    private void HandleSteamCmdOutput(Process process, bool commandIsMaintenance)
    {
        var input = process.StandardInput;
        var output = process.StandardOutput;
        var errors = process.StandardError;

        try
        {
            while (!process.HasExited)
            {
                var received = output.ReadLine();
                if (received is not null && received.ToLower().Contains("install state:"))
                    return;
                
                _logger.Verbose("STEAMCMD_OUTPUT[{ProcessId}]: {SteamCmdReceived}", process.Id, received);
            }

            _logger.Verbose("STEAMCMD_OUTPUT_ENDED[{ProcessId}]", process.Id);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occured during SteamCMD Output Handling: {Error}", ex.Message);
        }
        finally
        {
            input.Flush();
            input.Close();
            output.Close();
            errors.Close();

            if (commandIsMaintenance)
                SteamCmdUpdateInProgress = false;
        }
    }
    
    public async Task<IResult<SoftwareUpdateStatus>> CheckForUpdateGame(GameServerLocal gameServer)
    {
        // From Powershell server-watcher.ps1 script
        // +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir $GameServerPath +login anonymous +app_info_update 1 +app_update $steamAppId +app_status $steamAppId +quit
        //
        // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login anonymous +force_install_dir "ConanExiles" +app_info_update 1 +app_status "443030" +quit
        // See: https://steamcommunity.com/app/346110/discussions/0/535152511358957700/#c1768133742959565192
        
        // Another option, check game id via API: https://api.steamcmd.net/v1/info/$steamAppId
        //  $jsonContent = $response | ConvertFrom-Json
        //  $version = $jsonContent.data."$steamAppId".depots.branches.public.buildid
        //  $timeUpdated = $jsonContent.data."$steamAppId".depots.branches.public.timeupdated
        //  $epoch = [DateTime]::new(1970, 1, 1, 0, 0, 0, [DateTimeKind]::Utc)
        //  $releaseDate = $epoch.AddSeconds($timeUpdated)
        await Task.CompletedTask;
        
        // TODO: Update gameserver state to match post work state
        throw new NotImplementedException();
    }

    public async Task<IResult<SoftwareUpdateStatus>> CheckForUpdateMod(GameServerLocal gameServer, Mod mod)
    {
        // https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/?itemcount=1&publishedfileids%5B0%5D=3120364390
        // See: https://www.reddit.com/r/Steam/comments/30l5au/web_api_for_workshop_items/
        // response.publishedfiledetails.publishedfileid
        // response.publishedfiledetails.hcontent_file
        // response.publishedfiledetails.title
        // response.publishedfiledetails.time_created
        // response.publishedfiledetails.time_updated
        await Task.CompletedTask;
        
        // TODO: Update gameserver state to match post work state
        throw new NotImplementedException();
    }

    public async Task<IResult> InstallOrUpdateGame(GameServerLocal gameServer)
    {
        // See: https://steamcommunity.com/app/346110/discussions/0/535152511358957700/#c1768133742959565192
        try
        {
            if (!Directory.Exists(gameServer.GetInstallDirectory()))
            {
                Directory.CreateDirectory(gameServer.GetInstallDirectory());
                _logger.Information("Created directory for gameserver install: {Directory}", gameServer.GetInstallDirectory());
            }

            var installResult = await RunSteamCmdCommand(SteamConstants.CommandInstallUpdateGame(gameServer));
            if (!installResult.Succeeded)
            {
                _logger.Error("Failed to install/update gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
                return installResult;
            }
        
            _logger.Information("Successfully installed/updated gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            return installResult;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to install gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            return await Result.FailAsync($"Failed to install gameserver: [{gameServer.Id}]{gameServer.ServerName}");
        }
    }

    public async Task<IResult> InstallOrUpdateMod(GameServerLocal gameServer, Mod mod)
    {
        // steamcmd.exe +login anonymous +workshop_download_item 346110 496735411 +quit
        // steamcmd.exe +login anonymous +workshop_download_item {steamGameId} {workshopItemId} +quit
        // See: https://steamcommunity.com/app/346110/discussions/10/530649887212662565/?l=hungarian#c521643320353037920
        await Task.CompletedTask;
        
        // TODO: Update gameserver state to match post work state
        throw new NotImplementedException();
    }

    public async Task<IResult> UninstallGame(GameServerLocal gameServer)
    {
        try
        {
            _logger.Debug("Attempting to uninstall gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            Directory.Delete(gameServer.GetInstallDirectory(), true);
            _logger.Information("Successfully uninstalled gameserver[{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to uninstall gameserver: {Error}", ex.Message);
            return await Result.FailAsync($"Failed to uninstall gameserver: {ex.Message}");
        }
    }

    public async Task<IResult> UninstallMod(GameServerLocal gameServer, Mod mod)
    {
        // Delete mod directory and cleanup GameServer object
        await Task.CompletedTask;
        
        // TODO: Update gameserver state to match post work state
        throw new NotImplementedException();
    }

    private void DeleteOldBackups(string backupPath)
    {
        var backupDirectories = new DirectoryInfo(backupPath)
            .GetDirectories()
            .OrderBy(directory => directory.Name)
            .ToList();

        if (backupDirectories.Count <= _generalConfig.Value.GameserverBackupsToKeep) return;
        
        var directoriesToDelete = backupDirectories.Take(backupDirectories.Count - _generalConfig.Value.GameserverBackupsToKeep);
        foreach (var directory in directoriesToDelete)
        {
            try
            {
                directory.Delete(recursive: true);
                _logger.Debug("Deleted gameserver backup: {Directory}", directory.FullName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete gameserver backup: {Error}", ex.Message);
            }
        }
    }

    public async Task<IResult> BackupGame(GameServerLocal gameServer)
    {
        try
        {
            _logger.Debug("Starting backup for gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            
            var backupRootPath = Path.Combine(OsHelpers.GetDefaultBackupPath(), gameServer.Id.ToString());
            var backupTimestamp = _dateTimeService.NowDatabaseTime.ToString("yyyyMMdd_HHmm");
            var backupIndex = 0;
            foreach (var backupDirectory in gameServer.Resources.Where(x => x.Type == LocationType.BackupPath))
            {
                var backupPath = Path.Combine(gameServer.GetInstallDirectory(), backupDirectory.Path);
                var archivePath = Path.Combine(backupRootPath, backupTimestamp, $"{backupIndex}.zip");
                ZipFile.CreateFromDirectory(backupPath, archivePath, CompressionLevel.Optimal, includeBaseDirectory: false);
                backupIndex++;
                _logger.Information("Backed up path: {FilePath}", backupPath);
            }

            _logger.Information("Finished backing up gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            
            DeleteOldBackups(backupRootPath);
            
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred backing up gameserver: {GameserverId} | {Error}", gameServer.Id, ex.Message);
            return await Result.FailAsync($"Failure occurred backing up gameserver: {gameServer.Id} | {ex.Message}");
        }
    }
}