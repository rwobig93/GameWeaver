using System.Net;
using Application.Helpers;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.ControlServer;
using Domain.Models.Host;
using Microsoft.Extensions.Options;
using WeaverService.Helpers;

namespace WeaverService.Workers;

public class HostWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHostService _hostService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IWeaverWorkService _weaverWorkService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;
    private readonly IGameServerService _gameServerService;
    private readonly IHostApplicationLifetime _appLifetime;

    public static HostResourceUsage CurrentHostResourceUsage { get; private set; } = new();
    
    /// <summary>
    /// Handles host IO operations and resource usage gathering
    /// </summary>
    public HostWorker(ILogger logger, IHostService hostService, IDateTimeService dateTimeService, IWeaverWorkService weaverWorkService,
        IOptions<GeneralConfiguration> generalConfig, IGameServerService gameServerService, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _hostService = hostService;
        _dateTimeService = dateTimeService;
        _weaverWorkService = weaverWorkService;
        _generalConfig = generalConfig;
        _gameServerService = gameServerService;
        _appLifetime = appLifetime;
    }

    private static DateTime _lastRuntime;
    private static Task? _pollerThread;

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Started {ServiceName} service", nameof(HostWorker));
        Directory.CreateDirectory(_generalConfig.Value.AppDirectory);
        Directory.SetCurrentDirectory(_generalConfig.Value.AppDirectory);
        _logger.Information("Set application directory: {Directory}", _generalConfig.Value.AppDirectory);
        _logger.Information("  Full path: {Directory}", Directory.GetCurrentDirectory());
        _lastRuntime = _dateTimeService.NowDatabaseTime;
        ThreadHelper.ConfigureThreadPool(Environment.ProcessorCount, Environment.ProcessorCount * 2);
        
        var steamCmdStatus = await _gameServerService.ValidateSteamCmdInstall();
        if (!steamCmdStatus.Succeeded)
        {
            _logger.Fatal("SteamCMD validation failed, couldn't install/update SteamCMD, please check logs to troubleshoot");
            _logger.Debug("Stopping {ServiceName} service", nameof(HostWorker));
            _appLifetime.StopApplication();
        }
        
        // TODO: Implement control server consumption of host detail
        StartResourcePoller();
        
        // Add startup delay for poller details to fully gather
        var startupTimePassed = (_dateTimeService.NowDatabaseTime - _lastRuntime).Milliseconds;
        if (startupTimePassed < 3000)
            await Task.Delay(3000 - startupTimePassed, stoppingToken);
        
        // TODO: IPAddress object isn't serializable, need to look into options beyond what has already been tried
        // ThreadHelper.QueueWork(_ => UpdateHostDetail());
        
        // TODO: Implement Sqlite for host work tracking, for now we'll serialize/deserialize a json file
        await base.StartAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _lastRuntime = _dateTimeService.NowDatabaseTime;
                await UpdateCurrentResourceUsage();
            
                await ProcessWorkQueue();

                var millisecondsPassed = (_dateTimeService.NowDatabaseTime - _lastRuntime).Milliseconds;
                if (millisecondsPassed < _generalConfig.Value.HostWorkIntervalMs)
                    await Task.Delay(_generalConfig.Value.HostWorkIntervalMs - millisecondsPassed, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure occurred during {ServiceName} execution loop: {ErrorMessage}",
                    nameof(HostWorker), ex.Message);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.Debug("Stopping {ServiceName} service", nameof(HostWorker));

        await _weaverWorkService.Housekeeping();
        
        _logger.Debug("Stopped {ServiceName} service", nameof(HostWorker));
        await base.StopAsync(stoppingToken);
    }

    private void UpdateHostDetail()
    {
        _hostService.PollHostDetail();
        _hostService.PollHostResources();
        var currentHostInfo = _hostService.GetHardwareInfo();
        var hostDetailRequest = new HostDetailRequest
        {
            Os = new HostOperatingSystem
            {
                Name = currentHostInfo.OperatingSystem.Name,
                Version = currentHostInfo.OperatingSystem.VersionString
            }
        };
        
        currentHostInfo.MotherboardList.ForEach(motherboard => hostDetailRequest.Motherboards.Add(new HostMotherboard
        {
            Product = motherboard.Product,
            Manufacturer = motherboard.Manufacturer
        }));
        
        currentHostInfo.CpuList.ForEach(cpu => hostDetailRequest.Cpus.Add(new HostCpu
        {
            Name = cpu.Name,
            CoreCount = Convert.ToInt32(cpu.NumberOfCores),
            LogicalProcessorCount = Convert.ToInt32(cpu.NumberOfLogicalProcessors),
            SocketDesignation = cpu.SocketDesignation
        }));
        
        currentHostInfo.DriveList.ForEach(
            drive => drive.PartitionList.ForEach(
                partition => partition.VolumeList.ForEach(
                    volume => hostDetailRequest.Storage.Add(new HostStorage
                {
                    Index = drive.Index,
                    Model = drive.Model,
                    MountPoint = volume.Name,
                    Name = volume.VolumeName,
                    TotalSpace = volume.Size,
                    FreeSpace = volume.FreeSpace
                }))));
        
        currentHostInfo.NetworkAdapterList.ForEach(adapter => hostDetailRequest.NetworkInterfaces.Add(new HostNetworkInterface
        {
            Name = adapter.Description,
            Speed = adapter.Speed,
            Type = adapter.NetConnectionID,
            TypeDetail = adapter.AdapterType,
            MacAddress = adapter.MACAddress,
            IpAddresses = new SerializableList<IPAddress>(adapter.IPAddressList),
            DefaultGateways = new SerializableList<IPAddress>(adapter.DefaultIPGatewayList),
            DhcpServer = adapter.DHCPServer,
            DnsServers = new SerializableList<IPAddress>(adapter.DNSServerSearchOrderList)
        }));
        
        currentHostInfo.MemoryList.ForEach(ramStick => hostDetailRequest.RamModules.Add(new HostRam
        {
            Manufacturer = ramStick.Manufacturer,
            Speed = ramStick.Speed,
            Capacity = ramStick.Capacity
        }));
        
        currentHostInfo.VideoControllerList.ForEach(gpu => hostDetailRequest.Gpus.Add(new HostGpu
        {
            Name = gpu.Name,
            DriverVersion = gpu.DriverVersion,
            VideoMode = gpu.VideoModeDescription,
            AdapterRam = gpu.AdapterRAM
        }));
        
        new WeaverWork{Id = 0}.SendHostDetailUpdate(WeaverWorkState.Completed, hostDetailRequest);
    }

    private async Task UpdateCurrentResourceUsage()
    {
        if ((_dateTimeService.NowDatabaseTime - CurrentHostResourceUsage.TimeStamp).Seconds > 20)
        {
            _logger.Warning("Last resource gather was over 20 seconds ago, the poller must have died, restarting the poller");
            StartResourcePoller();
        }
        CurrentHostResourceUsage = _hostService.GetCurrentResourceUsage();
        await Task.CompletedTask;
    }

    private void StartResourcePoller()
    {
        if (_pollerThread is not null && !_pollerThread.IsCompleted)
        {
            _logger.Warning("Resource poller was asked to start when it is already running, skipping...");
            return;
        }
        
        _pollerThread = Task.Run(() =>
        {
            var pollerLastRun = _dateTimeService.NowDatabaseTime;
            
            while (true)
            {
                try
                {
                    _hostService.PollHostResources();
                    
                    // We'll ensure we are on a 2second interval for gathering
                    var millisecondsPassed = (_dateTimeService.NowDatabaseTime - pollerLastRun).Milliseconds;
                    if (millisecondsPassed < _generalConfig.Value.ResourceGatherIntervalMs)
                        Thread.Sleep(_generalConfig.Value.ResourceGatherIntervalMs - millisecondsPassed);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failure occurred running host resource poller: {Error}", ex.Message);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        });
    }

    private async Task ProcessWorkQueue()
    {
        var inProgressCount = await _weaverWorkService.GetCountInProgressHostAsync();
        var waitingCount = await _weaverWorkService.GetCountWaitingHostAsync();
        
        if (inProgressCount.Data >= _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            _logger.Verbose("In progress host work [{InProgressWork}] is at max [{MaxWork}], moving on | Waiting: {WaitingWork}",
                inProgressCount.Data, _generalConfig.Value.SimultaneousQueueWorkCountMax, waitingCount.Data);
            return;
        }
        
        if (waitingCount.Data < 1)
        {
            _logger.Verbose("No host work waiting, moving on");
            return;
        }
        
        // TODO: Add checking on in progress weaver work that hasn't had a heartbeat / update in awhile
        
        // Work is waiting, and we have available queue space, adding next work to the thread pool
        var attemptCount = 0;
        while (inProgressCount.Data < _generalConfig.Value.SimultaneousQueueWorkCountMax)
        {
            inProgressCount = await _weaverWorkService.GetCountInProgressHostAsync();
            waitingCount = await _weaverWorkService.GetCountWaitingHostAsync();
            if (waitingCount.Data < 1) break;
            
            if (attemptCount >= 5)
            {
                _logger.Error("Reached max attempt count in the host work queue, quiting cycle queue processing");
                return;
            }

            var nextWork = await _weaverWorkService.GetNextWaitingHostAsync();
            if (!nextWork.Succeeded || nextWork.Data is null)
            {
                _logger.Error("Failure occurred getting next host work from service: {Error}", nextWork.Messages);
                attemptCount++;
                continue;
            }

            var updateRequest = await _weaverWorkService.UpdateStatusAsync(nextWork.Data.Id, WeaverWorkState.InProgress);
            if (!updateRequest.Succeeded)
            {
                _logger.Error("Failure occurred updating host work from service: [{WeaverworkId}]{Error}", nextWork.Data.Id, updateRequest.Messages);
                attemptCount++;
                continue;
            }
            nextWork.Data.SendStatusUpdate(WeaverWorkState.InProgress, "Work is now in progress");
            
            // ReSharper disable once AsyncVoidLambda
            ThreadHelper.QueueWork(async _ => await HandleWork(nextWork.Data));
        }

        await Task.CompletedTask;
    }

    private async Task HandleWork(WeaverWork work)
    {
        try
        {
            switch (work.TargetType)
            {
                case >= WeaverWorkTarget.Host and < WeaverWorkTarget.GameServer:
                    _logger.Debug("Starting host work from queue: [{WorkId}]{WorkType}", work.Id, work.TargetType);
                    break;
                default:
                    _logger.Error("Invalid work type for work: [{WorkId}] of type {WorkType}", work.Id, work.TargetType);
                    await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
                    work.SendStatusUpdate(WeaverWorkState.Failed, "Host work data was invalid, please verify the payload");
                    return;
            }
            
            switch (work.TargetType)
            {
                case WeaverWorkTarget.HostStatusUpdate:
                    // TODO: Implement host status update
                    break;
                case WeaverWorkTarget.HostDetail:
                    UpdateHostDetail();
                    break;
                case WeaverWorkTarget.StatusUpdate:
                case WeaverWorkTarget.Host:
                case WeaverWorkTarget.GameServer:
                case WeaverWorkTarget.GameServerInstall:
                case WeaverWorkTarget.GameServerUpdate:
                case WeaverWorkTarget.GameServerUninstall:
                case WeaverWorkTarget.CurrentEnd:
                case WeaverWorkTarget.GameServerStateUpdate:
                case WeaverWorkTarget.GameServerStart:
                case WeaverWorkTarget.GameServerStop:
                case WeaverWorkTarget.GameServerRestart:
                default:
                    _logger.Error("Unsupported host work type asked for: Asked {WorkType}", work.TargetType);
                    await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
                    work.SendStatusUpdate(WeaverWorkState.Failed, $"Unsupported host work type asked for: {work.TargetType}");
                    return;
            }
            
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Completed);
            work.SendStatusUpdate(WeaverWorkState.Completed, "Work complete");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred handling host work: {Error}", ex.Message);
            await _weaverWorkService.UpdateStatusAsync(work.Id, WeaverWorkState.Failed);
            work.SendStatusUpdate(WeaverWorkState.Failed, $"Failure occurred handling host work: {ex.Message}");
        }
    }
}