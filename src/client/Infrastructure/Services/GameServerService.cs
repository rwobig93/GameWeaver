using System.Diagnostics;
using System.IO.Compression;
using Application.Constants;
using Application.Helpers;
using Application.Models;
using Application.Repositories;
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
    private readonly IGameServerRepository _gameServerRepository;

    public GameServerService(ILogger logger, IDateTimeService dateTimeService, IOptions<GeneralConfiguration> generalConfig, IGameServerRepository gameServerRepository)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _generalConfig = generalConfig;
        _gameServerRepository = gameServerRepository;
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
    
    public async Task<IResult<SoftwareUpdateStatus>> CheckForUpdateGame(Guid id)
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

    public async Task<IResult<SoftwareUpdateStatus>> CheckForUpdateMod(Guid id, Mod mod)
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

    public async Task<IResult<Guid>> Create(GameServerLocal gameServer)
    {
        var createRequest = await _gameServerRepository.CreateAsync(gameServer);
        if (!createRequest.Succeeded)
            return await Result<Guid>.FailAsync(createRequest.Messages);
        
        return await Result<Guid>.SuccessAsync(gameServer.Id);
    }

    public async Task<IResult<GameServerLocal?>> GetById(Guid id)
    {
        return await _gameServerRepository.GetByIdAsync(id);
    }

    public async Task<IResult<List<GameServerLocal>>> GetAll()
    {
        return await _gameServerRepository.GetAll();
    }

    public async Task<IResult> InstallOrUpdateGame(Guid id)
    {
        // See: https://steamcommunity.com/app/346110/discussions/0/535152511358957700/#c1768133742959565192
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
                return await Result.FailAsync(gameServerRequest.Messages);
            
            var gameServer = gameServerRequest.Data;
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
            _logger.Error(ex, "Failed to install gameserver: {GameserverId}", id);
            return await Result.FailAsync($"Failed to install gameserver: {id}");
        }
    }

    public async Task<IResult> InstallOrUpdateMod(Guid id, Mod mod)
    {
        // steamcmd.exe +login anonymous +workshop_download_item 346110 496735411 +quit
        // steamcmd.exe +login anonymous +workshop_download_item {steamGameId} {workshopItemId} +quit
        // See: https://steamcommunity.com/app/346110/discussions/10/530649887212662565/?l=hungarian#c521643320353037920
        await Task.CompletedTask;
        
        // TODO: Update gameserver state to match post work state
        throw new NotImplementedException();
    }

    public async Task<IResult> UninstallGame(Guid id)
    {
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
                return await Result.FailAsync(gameServerRequest.Messages);
            
            _logger.Debug("Attempting to uninstall gameserver: {GameserverId}", id);
            Directory.Delete(gameServerRequest.Data.GetInstallDirectory(), true);
            _logger.Information("Successfully uninstalled gameserver: {GameserverId}", id);
            return await _gameServerRepository.DeleteAsync(gameServerRequest.Data.Id);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to uninstall gameserver: {Error}", ex.Message);
            return await Result.FailAsync($"Failed to uninstall gameserver: {ex.Message}");
        }
    }

    public async Task<IResult> UninstallMod(Guid id, Mod mod)
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

    public async Task<IResult> BackupGame(Guid id)
    {
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
                return await Result.FailAsync(gameServerRequest.Messages);
            
            var gameServer = gameServerRequest.Data;
            var backupPaths = gameServer.Resources.Where(x => x.Type == LocationType.BackupPath).ToList();
            if (backupPaths.Count == 0)
            {
                _logger.Debug("Gameserver doesn't have a backup path configured, skipping: [{GameserverId}]{GameserverName}",
                    gameServer.Id, gameServer.ServerName);
                return await Result.SuccessAsync();
            }
            
            _logger.Debug("Starting backup for gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            
            var backupRootPath = Path.Combine(OsHelpers.GetDefaultBackupPath(), gameServer.Id.ToString());
            var backupTimestamp = _dateTimeService.NowDatabaseTime.ToString("yyyyMMdd_HHmm");
            var backupIndex = 0;
            foreach (var backupDirectory in backupPaths)
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
            _logger.Error(ex, "Failure occurred backing up gameserver: {GameserverId} | {Error}", id, ex.Message);
            return await Result.FailAsync($"Failure occurred backing up gameserver: {id} | {ex.Message}");
        }
    }

    public async Task<IResult> StartServer(Guid id)
    {
        var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
        if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
            return await Result.FailAsync(gameServerRequest.Messages);
            
        var gameServer = gameServerRequest.Data;
        var startupBinaries = gameServer.Resources.Where(x => x is {Startup: true, Type: LocationType.Executable}).ToList();
        var failures = new List<string>();

        foreach (var binary in startupBinaries)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(binary.Path))
                {
                    _logger.Error("Startup resource for gameserver has an empty path, I don't know what to start!: [{GameserverId}]{GameserverName}",
                        gameServer.Id, gameServer.ServerName);
                    failures.Add("Startup resource for gameserver has an empty path, I don't know what to start!");
                    continue;
                }
                
                var gameServerDirectory = gameServer.GetInstallDirectory();
                var fullPath = Path.Combine(gameServerDirectory, binary.Path);
                if (!fullPath.EndsWith(binary.Extension) && !string.IsNullOrWhiteSpace(binary.Extension))
                    fullPath = $"{fullPath}.{binary.Extension}";
            
                var startedProcess = Process.Start(new ProcessStartInfo
                {
                    Arguments = binary.Args,
                    CreateNoWindow = true,
                    FileName = fullPath,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = gameServerDirectory
                });
                if (startedProcess is null)
                {
                    _logger.Error("Gameserver process was started but exited already, likely due to a gameserver misconfiguration or bug: [{GameserverId}]{GameserverName}",
                        gameServer.Id, gameServer.ServerName);
                    failures.Add($"Gameserver process was started but exited already, likely due to a gameserver misconfiguration or bug: [{gameServer.Id}]{gameServer.ServerName}");
                    continue;
                }

                startedProcess.PriorityClass = ProcessPriorityClass.High;
            
                _logger.Information("Started process for gameserver: [{GameserverId}]{GameserverName}: {ProcessId}",
                    gameServer.Id, gameServer.ServerName, startedProcess.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred starting gameserver process [{GameserverId}]{GameserverName}: {ErrorMessage}",
                    gameServer.Id, gameServer.ServerName, ex.Message);
                failures.Add($"Failure occurred starting gameserver process [{gameServer.Id}]{gameServer.ServerName}: {ex.Message}");
            }
        }

        if (failures.Count != 0)
            return await Result.FailAsync(failures);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> StopServer(Guid id)
    {
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
                return await Result.FailAsync(gameServerRequest.Messages);
            
            var gameServer = gameServerRequest.Data;
            var gameserverProcesses = OsHelpers.GetProcessesByDirectory(gameServer.GetInstallDirectory()).ToList();
            if (gameserverProcesses.Count == 0)
            {
                _logger.Debug("Found no running processes from the gameserver directory to stop: [{GameserverId}]{GameserverName}",
                    gameServer.Id, gameServer.ServerName);
                return await Result.SuccessAsync();
            }

            foreach (var process in gameserverProcesses)
                process.Kill();

            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop gameserver processes [{GameserverId}]: {ErrorMessage}", id, ex.Message);
            return await Result.FailAsync($"Failed to stop gameserver processes [{id}]: {ex.Message}");
        }
    }

    public async Task<IResult> Update(GameServerLocalUpdate gameServerUpdate)
    {
        return await _gameServerRepository.UpdateAsync(gameServerUpdate);
    }

    public async Task<IResult> UpdateState(Guid id, ServerState state)
    {
        return await _gameServerRepository.UpdateAsync(new GameServerLocalUpdate {Id = id, ServerState = state});
    }

    public async Task<IResult> Housekeeping()
    {
        return await _gameServerRepository.SaveAsync();
    }

    public async Task<IResult<bool>> IsServerStateAsExpected(Guid id)
    {
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
                return await Result<bool>.FailAsync(gameServerRequest.Messages);
            
            var gameServer = gameServerRequest.Data;
            var gameserverProcesses = OsHelpers.GetProcessesByDirectory(gameServer.GetInstallDirectory()).ToList();
            
            // TODO: Find a cross platform way to get listening TCP & UDP connections and their processes, only current way I could find is using per OS binaries
            // State.InternallyConnectable | Check for process listening on gameserver ports and verify the running directory for those processes
            
            // State.Shutdown | Check if any processes are running from directory
            if (gameServer.ServerState is ServerState.Shutdown or ServerState.Uninstalling && gameserverProcesses.Count == 0)
            {
                _logger.Verbose("Gameserver matches it's shutdown state: [{GameserverId}]{GameserverState}", gameServer.Id, gameServer.ServerState);
                return await Result<bool>.SuccessAsync(true);
            }
            
            if (gameServer.ServerState is ServerState.Shutdown or ServerState.Uninstalling && gameserverProcesses.Count != 0)
            {
                await UpdateState(gameServer.Id, ServerState.SpinningUp);
                _logger.Verbose("Gameserver doesn't matches it's shutdown state: [{GameserverId}]{GameserverState}", gameServer.Id, ServerState.SpinningUp);
                return await Result<bool>.SuccessAsync(false);
            }
            
            if (gameServer.ServerState is ServerState.Connectable or
                ServerState.Installing or
                ServerState.Restarting or
                ServerState.Updating or
                ServerState.InternallyConnectable or
                ServerState.SpinningUp)
            {
                
            }
            
            // State.Installing-or-Updating-or-Restarting | Verify processes are running from directory
            if (gameserverProcesses.Count == 0)
            {
                await UpdateState(gameServer.Id, ServerState.Shutdown);
                _logger.Verbose("Gameserver doesn't matches it's running state: [{GameserverId}]{GameserverState}", gameServer.Id, ServerState.Shutdown);
                return await Result<bool>.SuccessAsync(false);
            }
            
            // State.Stalled | If was connectable before and now is running while not listening
            // TODO: State.Stalled | Set spin up time limit for stall and if it was connectable before and now is running while not listening, stalled is conveyed
            
            return await Result<bool>.SuccessAsync(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred checking on local game server state [{GameserverId}]: {Error}", id, ex.Message);
            return await Result<bool>.FailAsync($"Failure occurred checking on local game server state [{id}]: {ex.Message}");
        }
    }
}