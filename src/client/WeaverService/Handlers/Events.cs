using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Domain.Models;
using Serilog;
using WeaverService.Extensions;
using WeaverService.Server;

namespace WeaverService.Handlers;

public static class Events
{
    public static void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        Log.Debug($@"File changed: [type]{e.ChangeType} [name]{e.Name} [path]{e.FullPath}");
        Housekeeping.ValidateGameWatchChange(e);
    }

    internal static void Watcher_Created(object sender, FileSystemEventArgs e)
    {
        Log.Debug($@"File Created: [type]{e.ChangeType} [name]{e.Name} [path]{e.FullPath}");
        Housekeeping.ValidateGameWatchCreate(e);
    }

    public static void SteamCMD_Exited(object sender, EventArgs e)
    {
        Log.Verbose("SteamCMDCLI: Exited");
    }

    public static async Task SteamCMD_Exit_UpdateGameServerState(object sender, EventArgs e, GameServerLocal gameServerLocal)
    {
        // Some gameservers require a first run for dependencies to be initialized, we'll do that now for a better user experience for a new install
        try
        {
            if (gameServerLocal.ServerState == ServerState.Installing)
            {
                gameServerLocal.StartServer();
                await Task.Delay(1000 * 20);
                gameServerLocal.StopServer();
            }
            gameServerLocal.UpdatesWaiting.RemoveAll(x => x.Source == GameSource.Manual || x.Source == GameSource.Steam);
            await gameServerLocal.UpdateServerState(ServerState.Shutdown);

            await API.UpdateGameServerRemote(gameServerLocal.Id, new Peiskos.Shared.Requests.GSM.GameServerEditRequest()
            {
                Id = gameServerLocal.Id,
                UpdatesWaiting = gameServerLocal.UpdatesWaiting,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occured attempting to update the gameserver state after a install/update/state change: {ErrorMessage}", ex.Message);
        }
    }

    public static void SteamCMD_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log.Verbose($"SteamCMDCLI: {e.Data}");
    }

    public static void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        Log.Information($"{sender} Download Finished");
    }

    internal static void APIWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        Log.Verbose($"API worker progress changed: {e.ProgressPercentage}");
    }

    internal static void APIWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Information("API worker completed and stopped");
    }

    internal static async Task APIWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting API worker main thread");
        HttpClient apiClient = new HttpClient();
        DateTime lastServerReachabilityTime = new DateTime(1900, 01, 01);
        while (Constants.WorkerAPIRunning)
        {
            await PeiskosCheckInMain(apiClient, lastServerReachabilityTime);
        }
        apiClient.Dispose();
        Log.Information("Exiting API worker main thread");
    }

    private static async Task PeiskosCheckInMain(HttpClient apiClient, DateTime lastServerReachabilityTime)
    {
        var serverReachable = await API.ServerIsUp(_httpClient: apiClient);
        if (string.IsNullOrWhiteSpace(Constants.Config.HomeServerSocket))
        {
            // Wait 5 seconds if we still haven't set a valid Peiskos Server Socket
            Thread.Sleep(5000);
        }
        else if (Housekeeping.CompanionHasPeiskosLogin() && serverReachable)
        {
            lastServerReachabilityTime = DateTime.Now;
            var checkinRequest = await API.PeiskosCheckIn(apiClient);
            if (!checkinRequest)
            {
                var waitTime = (Constants.Config.CheckInInterval * 5);
                Log.Debug("Host isn't joined or the SHA account is disabled, will attempt again in {WaitTime} seconds", (waitTime / 1000));
                Thread.Sleep(waitTime);
            }
            Log.Verbose("Checkin Request Success: {PeiskosCheckIn}", checkinRequest);
            Thread.Sleep(Constants.Config.CheckInInterval);
        }
        else if (!Housekeeping.CompanionHasPeiskosLogin() && serverReachable)
        {
            await API.ValidateHostJoinRequest(_httpClient: apiClient);
            Thread.Sleep(Constants.Config.CheckInInterval * 5);
        }
        else
        {
            if (DateTime.Now > lastServerReachabilityTime.AddMinutes(10))
                Log.Information("Peiskos server has been down or unreachable for 10 min or more, will continue trying to connect | Minutes Down: {MinutesUnreachable} | Last Reachable: {LastReachabilityTime}", (DateTime.Now - lastServerReachabilityTime).Minutes, lastServerReachabilityTime);
            // Wait 5 seconds if we are waiting for server connectivity to be restored to check again
            Log.Debug("Waiting for server reachability: {PeiskosJoined} | {PeiskosReachable}", Housekeeping.CompanionHasPeiskosLogin(), serverReachable);
            Thread.Sleep(5000);
        }
    }

    internal static void CommandWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Information("Command worker completed and stopped");
    }

    internal static async Task CommandWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting Command worker main thread");
        while (Constants.WorkerCommandRunning)
        {
            await HandleCommandList();
        }
        Log.Information("Exiting Command worker main thread");
    }

    [DisableConcurrentExecution(2)]
    private static async Task HandleCommandList()
    {
        while (!Constants.CommandQueue.IsEmpty)
        {
            CompanionCommand command;
            if (Constants.CommandQueue.TryTake(out command))
            {
                Log.Debug("Processing Host Command: [{CommandID}]{CommandType} | {HostID}", command.Id, nameof(command.CommandType), command.HostId);
                if (command.IsWaitingOnSomething())
                    Log.Debug("Removing command from queue and will pick it up again as we're waiting on it's resource: ", command.Id, command.CommandType);
                else
                {
                    switch (command.CommandType)
                    {
                        case HostCommand.RestartHost:
                            await CommandProcessing.RestartHost(command);
                            break;
                        case HostCommand.UpdateHost:
                            await CommandProcessing.UpdateHost(command);
                            break;
                        case HostCommand.ReconfigureHost:
                            await CommandProcessing.ReconfigureHost(command);
                            break;
                        case HostCommand.RestartGameServer:
                            await CommandProcessing.RestartGameServer(command);
                            break;
                        case HostCommand.StartGameServer:
                            await CommandProcessing.StartGameServer(command);
                            break;
                        case HostCommand.StopGameServer:
                            await CommandProcessing.StopGameServer(command);
                            break;
                        case HostCommand.UpdateGameServer:
                            await CommandProcessing.UpdateGameServer(command);
                            break;
                        case HostCommand.ReconfigureGameServer:
                            await CommandProcessing.ReconfigureGameServer(command);
                            break;
                        case HostCommand.CreateGameServer:
                            await CommandProcessing.CreateGameServer(command);
                            break;
                        case HostCommand.DeleteGameServer:
                            await CommandProcessing.DeleteGameServer(command);
                            break;
                        default:
                            await CommandProcessing.UnknownCommand(command);
                            break;
                    }
                }
            }
            else
                Log.Debug("Failed to grab command from command queue, will attempt again | Queue Length: {CommandQueueLength}", Constants.CommandQueue.Count);
        }
    }

    internal static void CommandWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        Log.Verbose($"Command worker progress changed: {e.ProgressPercentage}");
    }

    public static void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        Log.Debug($"{sender} Download Progress: {e.ProgressPercentage}% {e.BytesReceived}\\{e.TotalBytesToReceive}");
    }
    public static void TelemetryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Information("Telemetry worker completed and stopped");
    }

    public static void TelemetryWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Starting TelemetryWorker_DoWork()");
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        cpuCounter.NextValue();
        Log.Debug("Initialized cpuCounter");
        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        ramCounter.NextValue();
        Log.Debug("Initialized ramCounter");
        PerformanceCounter uptCounter = new PerformanceCounter("System", "System Up Time");
        uptCounter.NextValue();
        Log.Debug("Initialized uptCounter");
        int progress = 0;
        if (Constants.TelemetryBag == null || Constants.TelemetryBag.Count < 1)
        {
            Log.Debug("telemetryBag is null or empty, initializing and adding object");
            //Constants.TelemetryBag = new System.Collections.Concurrent.ConcurrentBag<NatDtoTelemetryInfo>
            //{
            //    new NatDtoTelemetryInfo()
            //    {
            //        Cpu = cpuCounter.NextValue(),
            //        Ram = ramCounter.NextValue(),
            //        Uptime = uptCounter.NextValue()
            //    }
            //};
            Log.Debug("Finished initializing and adding to telemetryBag");
        }
        Log.Information("Finished initializing system health objects");
        while (Constants.WorkerTelemetryRunning)
        {
            Thread.Sleep(1000);
            //Constants.TelemetryBag.First().Cpu = cpuCounter.NextValue();
            //Constants.TelemetryBag.First().Ram = ramCounter.NextValue();
            //Constants.TelemetryBag.First().Uptime = uptCounter.NextValue();
            if (progress > 60)
            {
                Log.Debug($"Telemetry Worker Progress is {progress}, resetting");
                progress = 0;
            }
            progress++;
            Constants.TelemetryWorker.ReportProgress(progress);
        }
        Log.Debug("Left TelemetryWorker_DoWork while loop");
        cpuCounter.Dispose();
        Log.Debug("Disposed cpuCounter");
        ramCounter.Dispose();
        Log.Debug("Disposed ramCounter");
        uptCounter.Dispose();
        Log.Debug("Finished disposing performance counters");
    }

    public static void TelemetryWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        Log.Verbose($"Telemetry worker progress changed: {e.ProgressPercentage}");
        //if (e.ProgressPercentage <= 61)
        //{
        //    Log.Verbose("Telemetry worker progress value is send home server update");
        //    if (Constants.NatConn.State != ConnState.CLOSED ||
        //        Constants.NatConn.State != ConnState.CONNECTING ||
        //        Constants.NatConn.State != ConnState.DISCONNECTED ||
        //        Constants.NatConn.State != ConnState.RECONNECTING)
        //    {
        //        //NatDtoTelemetryInfo telemDto = new NatDtoTelemetryInfo()
        //        //{
        //        //    CompanionID = Constants.Config.CompanionID,
        //        //    Cpu = Constants.TelemetryBag.First().Cpu,
        //        //    Ram = Constants.TelemetryBag.First().Ram,
        //        //    Uptime = Constants.TelemetryBag.First().Uptime
        //        //};
        //        //Log.Verbose("Sending telemetry");
        //        //Constants.NatConn.Publish(NatSubjects.Telemetry, NatComm.NatMsgSend(NatCommType.Health, telemDto));
        //    }
        //    else
        //    {
        //        Log.Debug($"Not sending telemetry, connection is {Constants.NatConn.State}");
        //    }
        //}
    }
}
