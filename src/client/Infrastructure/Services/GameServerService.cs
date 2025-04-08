using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Application.Constants;
using Application.Helpers;
using Application.Models.GameServer;
using Application.Repositories;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;
using GameWeaverShared.Parsers;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class GameServerService : IGameServerService
{
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;
    private readonly IGameServerRepository _gameServerRepository;
    private readonly ISerializerService _serializerService;
    private readonly IControlServerService _serverService;

    public GameServerService(ILogger logger, IDateTimeService dateTimeService, IOptions<GeneralConfiguration> generalConfig, IGameServerRepository gameServerRepository,
        ISerializerService serializerService, IControlServerService serverService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _generalConfig = generalConfig;
        _gameServerRepository = gameServerRepository;
        _serializerService = serializerService;
        _serverService = serverService;
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
            var steamCmd = new Process
            {
                StartInfo = new ProcessStartInfo
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
                if (received is not null && received.Contains("install state:", StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }

                _logger.Debug("STEAMCMD_OUTPUT[{ProcessId}]: {SteamCmdReceived}", process.Id, received);
            }

            _logger.Debug("STEAMCMD_OUTPUT_ENDED[{ProcessId}]", process.Id);
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
            {
                SteamCmdUpdateInProgress = false;
            }
        }
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

    public async Task<IResult> InstallOrUpdateGame(Guid id, bool validate = false)
    {
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
            {
                return await Result.FailAsync(gameServerRequest.Messages);
            }

            var gameServer = gameServerRequest.Data;
            if (!Directory.Exists(gameServer.GetInstallDirectory()))
            {
                Directory.CreateDirectory(gameServer.GetInstallDirectory());
                _logger.Information("Created directory for gameserver install: {Directory}", gameServer.GetInstallDirectory());
            }

            return gameServer.Source switch
            {
                GameSource.Steam => await InstallOrUpdateGameSteam(validate, gameServer),
                GameSource.Manual => await InstallOrUpdateGameManual(gameServer),
                _ => await Result.FailAsync($"Gameserver has an invalid source, unable to install: {id}")
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to install gameserver: {GameserverId}", id);
            return await Result.FailAsync($"Failed to install gameserver: {id}");
        }
    }

    private async Task<IResult> InstallOrUpdateGameManual(GameServerLocal gameServer, bool secondRun = false)
    {
        var serverClientDownload = await _serverService.DownloadManualClient(gameServer.GameId);
        if (!serverClientDownload.Succeeded)
        {
            _logger.Error("Failed to install/update gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            {
                return serverClientDownload;
            }
        }
        _logger.Debug("Downloaded manual game server client, now saving to the local file system: [{GameserverId}]{GameserverName} => {Filename}",
            gameServer.Id, gameServer.ServerName, serverClientDownload.Data.FileName);

        var clientDownloadPath = Path.Join(gameServer.GetInstallDirectory(), serverClientDownload.Data.FileName);
        if (File.Exists(clientDownloadPath))
        {
            File.Delete(clientDownloadPath);
            _logger.Debug("Deleted old manual server client: [{GameserverId}]{GameserverName} => {Filename}",
                gameServer.Id, gameServer.ServerName, serverClientDownload.Data.FileName);
        }

        File.WriteAllBytes(clientDownloadPath, serverClientDownload.Data.Content);
        _logger.Debug("Successfully saved game server client to the local file system: [{GameserverId}]{GameserverName} => {Filename}",
            gameServer.Id, gameServer.ServerName, serverClientDownload.Data.FileName);

        var downloadedHash = FileHelpers.ComputeFileContentSha256Hash(clientDownloadPath);
        if (downloadedHash != serverClientDownload.Data.HashSha256)
        {
            if (secondRun)
            {
                _logger.Error("Second install / update failed and hash doesn't match: [{GameserverId}]{GameserverName} => {Filename}",
                    gameServer.Id, gameServer.ServerName, serverClientDownload.Data.FileName);
                _logger.Error("  {Filename} expected hash: {IntegrityHash}", serverClientDownload.Data.FileName, serverClientDownload.Data.HashSha256);
                _logger.Error("  {Filename} actual hash:   {IntegrityHash}", serverClientDownload.Data.FileName, downloadedHash);
                return await Result.FailAsync(
                    $"Second install / update failed and hash doesn't match [{gameServer.Id}]{gameServer.ServerName} => {serverClientDownload.Data.FileName}");
            }

            _logger.Error("Integrity hash doesn't match for downloaded file: [{GameserverId}]{GameserverName} => {Filename}",
                gameServer.Id, gameServer.ServerName, serverClientDownload.Data.FileName);
            _logger.Error("  {Filename} expected hash: {IntegrityHash}", serverClientDownload.Data.FileName, serverClientDownload.Data.HashSha256);
            _logger.Error("  {Filename} actual hash:   {IntegrityHash}", serverClientDownload.Data.FileName, downloadedHash);
            return await InstallOrUpdateGameManual(gameServer, true);
        }

        if (serverClientDownload.Data.Format != FileStorageFormat.ArchiveZip) return serverClientDownload;

        ZipFile.ExtractToDirectory(clientDownloadPath, gameServer.GetInstallDirectory());
        _logger.Debug("Extracted zip archive directly into game server install directory: [{GameserverId}]{GameserverName} => {Filename}",
            gameServer.Id, gameServer.ServerName, serverClientDownload.Data.FileName);

        return serverClientDownload;
    }

    private async Task<IResult> InstallOrUpdateGameSteam(bool validate, GameServerLocal gameServer)
    {
        // See: https://steamcommunity.com/app/346110/discussions/0/535152511358957700/#c1768133742959565192
        var steamInstall = await RunSteamCmdCommand(SteamConstants.CommandInstallUpdateGame(gameServer, validate));
        if (!steamInstall.Succeeded)
        {
            _logger.Error("Failed to install/update gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
            {
                return steamInstall;
            }
        }

        _logger.Information("Successfully installed/updated gameserver: [{GameserverId}]{GameserverName}", gameServer.Id, gameServer.ServerName);
        return steamInstall;
    }

    public async Task<IResult> InstallOrUpdateMod(Guid id, Mod mod)
    {
        // steamcmd.exe +login anonymous +workshop_download_item 346110 496735411 +quit
        // steamcmd.exe +login anonymous +workshop_download_item {steamGameId} {workshopItemId} +quit
        // See: https://steamcommunity.com/app/346110/discussions/10/530649887212662565/?l=hungarian#c521643320353037920
        await Task.CompletedTask;
        return await Result.FailAsync("This method isn't implemented yet");
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
        return await Result.FailAsync("This method isn't implemented yet");
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
            var backupPaths = gameServer.Resources.Where(x => x.Type == ResourceType.BackupPath).ToList();
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
        var startupResources = gameServer.Resources.Where(x => x is {Startup: true, Type: ResourceType.Executable or ResourceType.ScriptFile}).ToList();
        var failures = new List<string>();

        foreach (var resource in startupResources)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resource.Path))
                {
                    _logger.Error("Startup resource for gameserver has an empty path, I don't know what to start!: [{GameserverId}]{GameserverName}",
                        gameServer.Id, gameServer.ServerName);
                    failures.Add("Startup resource for gameserver has an empty path, I don't know what to start!");
                    continue;
                }

                var gameServerDirectory = gameServer.GetInstallDirectory();
                var fullPath = Path.Combine(gameServerDirectory, FileHelpers.SanitizeSecureFilename(resource.Path));

                if (!File.Exists(fullPath))
                {
                    _logger.Error("Startup resource for gameserver doesn't exist: [{GameserverId}]{GameserverName} => {FilePath}",
                        gameServer.Id, gameServer.ServerName, fullPath);
                    failures.Add($"Startup resource for gameserver doesn't exist: [{gameServer.Id}]{gameServer.ServerName} => {fullPath}");
                    continue;
                }

                var startedProcess = Process.Start(new ProcessStartInfo
                {
                    Arguments = gameServer.UpdateWithServerValues(resource.Args),
                    CreateNoWindow = true,
                    FileName = fullPath,
                    UseShellExecute = resource.Type is ResourceType.ScriptFile,
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

    public async Task<IResult> UpdateState(Guid id, ServerState state, string storageConfigHash)
    {
        return await _gameServerRepository.UpdateAsync(new GameServerLocalUpdate {Id = id, ServerState = state, StorageConfigHash = storageConfigHash});
    }

    public async Task<IResult> UpdateState(Guid id, ServerState state, string runningConfigHash, string storageConfigHash)
    {
        return await _gameServerRepository.UpdateAsync(new GameServerLocalUpdate
        {
            Id = id,
            ServerState = state,
            RunningConfigHash = runningConfigHash,
            StorageConfigHash = storageConfigHash
        });
    }

    public async Task<IResult> Housekeeping()
    {
        return await _gameServerRepository.SaveAsync();
    }

    public async Task<IResult<ServerState>> GetCurrentRealtimeState(Guid id)
    {
        try
        {
            var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
            if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
            {
                return await Result<ServerState>.FailAsync(gameServerRequest.Messages);
            }

            var gameServer = gameServerRequest.Data;

            if (!Directory.Exists(gameServer.GetInstallDirectory()))
            {
                return await Result<ServerState>.SuccessAsync(ServerState.Uninstalled);
            }

            var gameserverProcesses = OsHelpers.GetProcessesByDirectory(gameServer.GetInstallDirectory()).ToList();
            var gameserverListeningSockets = gameServer.GetListeningSockets();

            switch (gameserverProcesses.Count)
            {
                case > 0 when gameserverListeningSockets.Count > 0:
                    return await Result<ServerState>.SuccessAsync(ServerState.InternallyConnectable);
                case > 0 when gameserverListeningSockets.Count <= 0:
                    return await Result<ServerState>.SuccessAsync(ServerState.Stalled);
                case <= 0:
                    return await Result<ServerState>.SuccessAsync(ServerState.Shutdown);
                default:
                    _logger.Error("Failed to properly calculate local server state: [{GameserverId}]{GameserverName} | [Processes]{ProcessCount} [Sockets]{SocketCount}",
                        gameServer.Id, gameServer.ServerName, gameserverProcesses.Count, gameserverListeningSockets.Count);
                    return await Result<ServerState>.FailAsync(ServerState.Unknown);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred checking on local game server state [{GameserverId}]: {Error}", id, ex.Message);
            return await Result<ServerState>.FailAsync($"Failure occurred checking on local game server state [{id}]: {ex.Message}");
        }
    }

    private async Task<IResult> UpdateIniFile(LocalResource configFile, bool loadExisting = true)
    {
        var filePath = configFile.GetFullPath();
        var allowDuplicates = configFile.ConfigSets.Any(x => x.DuplicateKey);
        var configFileContent = new IniData(allowDuplicates: allowDuplicates);

        try
        {
            if (loadExisting && File.Exists(filePath))
            {
                configFileContent.Load(filePath, true);
                _logger.Debug("Existing ini file exists, loaded config contents: {FilePath}", filePath);
            }

            var iniFromResource = configFile.ConfigSets.ToIni();
            configFileContent.AggregateFrom(iniFromResource);

            var saveRequest = await configFileContent.Save(filePath);
            if (!saveRequest.Succeeded)
            {
                foreach (var message in saveRequest.Messages)
                {
                    _logger.Debug("Ini config file save failure: {Error}", message);
                }

                return await Result.FailAsync();
            }

            _logger.Debug("Saved ini config file at {FilePath}", filePath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred updating {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
            return await Result.FailAsync($"Failure occurred updating {configFile.ContentType} config file: {ex.Message}");
        }
    }

    private async Task<IResult> UpdateJsonFile(LocalResource configFile, bool loadExisting = true)
    {
        var filePath = configFile.GetFullPath();
        var configFileContent = new Dictionary<string, string>();

        if (loadExisting && File.Exists(filePath))
        {
            _logger.Debug("Existing config file found and desired to merge, attempting to load: {FilePath}", filePath);
            try
            {
                var loadedContent = File.ReadAllText(filePath);
                configFileContent = _serializerService.DeserializeJson<Dictionary<string, string>>(loadedContent);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load existing {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
            }
        }

        configFileContent.AggregateJsonReadyFrom(configFileContent);

        try
        {
            var serializedContent = _serializerService.SerializeJson(configFileContent);
            File.WriteAllText(filePath, serializedContent, Encoding.UTF8);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred updating {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
            return await Result.FailAsync($"Failure occurred updating {configFile.ContentType} config file: {ex.Message}");
        }
    }

    private async Task<IResult> UpdateXmlFile(LocalResource configFile, bool loadExisting = true)
    {
        var filePath = configFile.GetFullPath();

        var rootElement = configFile.ConfigSets.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Path));
        if (rootElement is null)
        {
            return await Result.FailAsync("XML config file is missing at least one root element, unable to create config file");
        }

        var xmlDocument = new XDocument(new XElement(rootElement.Key));
        if (loadExisting && File.Exists(filePath))
        {
            _logger.Debug("Existing config file found and desired to merge, attempting to load: {FilePath}", filePath);
            try
            {
                xmlDocument = XDocument.Load(filePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load existing {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
                return await Result.FailAsync($"Failed to load existing {configFile.ContentType} config file: {ex.Message}");
            }
        }

        if (xmlDocument.Root is null)
        {
            return await Result.FailAsync("XML Document is missing a root element, unable to modify or save XML file");
        }

        var resourceConfigXml = configFile.ConfigSets.ToXml();
        if (resourceConfigXml?.Root is null)
        {
            return await Result.FailAsync("Failed to convert local resource config to a valid XML file, likely due to missing root element");
        }

        // Merge any existing loaded XML or our elements into the empty XML document if nothing has been loaded
        xmlDocument.Root.Add(resourceConfigXml.Root.Elements());

        try
        {
            xmlDocument.Save(filePath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred updating {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
            return await Result.FailAsync($"Failure occurred updating {configFile.ContentType} config file: {ex.Message}");
        }
    }

    private async Task<IResult> UpdateRawFile(LocalResource configFile, bool loadExisting = false)
    {
        var filePath = configFile.GetFullPath();
        List<string> fileContent = [];

        // Merging / 'loadExisting' for Raw/text files is unlikely to ever be useful, but we have the option in case it ever is
        if (loadExisting && File.Exists(filePath))
        {
            _logger.Debug("Existing config file found and desired to merge, attempting to load: {FilePath}", filePath);
            try
            {
                fileContent = [..File.ReadAllLines(filePath)];
                fileContent.AggregateRawFrom(configFile.ConfigSets);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load existing {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
            }
        }

        if (!loadExisting || !File.Exists(filePath))
        {
            fileContent = configFile.ConfigSets.ToRaw();
        }

        try
        {
            File.WriteAllLines(filePath, fileContent, Encoding.UTF8);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred updating {ContentType} config file: {Error}", configFile.ContentType, ex.Message);
            return await Result.FailAsync($"Failure occurred updating {configFile.ContentType} config file: {ex.Message}");
        }
    }

    public async Task<IResult> UpdateConfigItemFiles(Guid id, bool loadExisting = true)
    {
        var gameServerRequest = await _gameServerRepository.GetByIdAsync(id);
        if (!gameServerRequest.Succeeded || gameServerRequest.Data is null)
        {
            return await Result.FailAsync(gameServerRequest.Messages);
        }

        List<string> errorMessages = [];
        var configItemFiles = gameServerRequest.Data.Resources.Where(x =>
            x.Type is ResourceType.ConfigFile or ResourceType.ScriptFile).ToList();

        foreach (var configFile in configItemFiles)
        {
            if (configFile.ContentType is ContentType.Deleted)  // Local resource is an ignore file so we'll delete it since we don't want it
            {
                try
                {
                    if (File.Exists(configFile.GetFullPath()))
                    {
                        _logger.Debug("File is a delete file and exists, deleting: {FilePath}", configFile.GetFullPath());
                        File.Delete(configFile.GetFullPath());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to delete ignore file: {Error}", ex.Message);
                    return await Result.FailAsync($"Failed to delete ignore file: {ex.Message}");
                }

                continue;
            }

            try
            {
                var directoryPath = Path.GetDirectoryName(configFile.GetFullPath()) ?? "./";
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to ensure config path exists: {Error}", ex.Message);
                return await Result.FailAsync($"Failed to ensure config path exists: {ex.Message}");
            }

            foreach (var item in configFile.ConfigSets)
            {
                item.Value = gameServerRequest.Data.UpdateWithServerValues(item.Value);
            }

            switch (configFile)
            {
                case {ContentType: ContentType.Json}:
                {
                    var jsonUpdateRequest = await UpdateJsonFile(configFile, configFile.LoadExisting);
                    if (!jsonUpdateRequest.Succeeded)
                    {
                        errorMessages.AddRange(jsonUpdateRequest.Messages);
                    }

                    continue;
                }
                case {ContentType: ContentType.Ini}:
                {
                    var iniUpdateRequest = await UpdateIniFile(configFile, configFile.LoadExisting);
                    if (!iniUpdateRequest.Succeeded)
                    {
                        errorMessages.AddRange(iniUpdateRequest.Messages);
                    }

                    continue;
                }
                case {ContentType: ContentType.Xml}:
                {
                    var xmlUpdateRequest = await UpdateXmlFile(configFile, configFile.LoadExisting);
                    if (!xmlUpdateRequest.Succeeded)
                    {
                        errorMessages.AddRange(xmlUpdateRequest.Messages);
                    }
                    continue;
                }
                case {ContentType: ContentType.Raw}:
                {
                    var rawUpdateRequest = await UpdateRawFile(configFile, configFile.LoadExisting);
                    if (!rawUpdateRequest.Succeeded)
                    {
                        errorMessages.AddRange(rawUpdateRequest.Messages);
                    }
                    continue;
                }
                default:
                {
                    errorMessages.Add($"Config file content type '{configFile.ContentType}' isn't currently supported");
                    continue;
                }
            }
        }

        if (errorMessages.Count > 0)
        {
            return await Result.FailAsync(errorMessages);
        }

        return await Result.SuccessAsync();
    }
}