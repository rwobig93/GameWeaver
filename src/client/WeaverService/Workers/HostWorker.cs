using System.Collections.Concurrent;
using Application.Helpers;
using Application.Requests.Host;
using Application.Services;
using Application.Settings;
using Domain.Enums;
using Domain.Models.ControlServer;
using Domain.Models.Host;
using Microsoft.Extensions.Options;

namespace WeaverService.Workers;

public class HostWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHostService _hostService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ISerializerService _serializerService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;
    private readonly IGameServerService _gameServerService;
    private readonly IHostApplicationLifetime _appLifetime;

    private static readonly ConcurrentQueue<WeaverWorkClient> WorkInProgressQueue = new();
    private static readonly ConcurrentQueue<WeaverWorkClient> WorkWaitingQueue = new();
    private static DateTime _lastRuntime;
    private static Task? _pollerThread;

    public static HostResourceUsage CurrentHostResourceUsage { get; private set; } = new();
    
    /// <summary>
    /// Handles host IO operations and resource usage gathering
    /// </summary>
    public HostWorker(ILogger logger, IHostService hostService, IDateTimeService dateTimeService, ISerializerService serializerService,
        IOptions<GeneralConfiguration> generalConfig, IGameServerService gameServerService, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _hostService = hostService;
        _dateTimeService = dateTimeService;
        _serializerService = serializerService;
        _generalConfig = generalConfig;
        _gameServerService = gameServerService;
        _appLifetime = appLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
        
        ThreadHelper.QueueWork(_ => UpdateHostDetail());
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _lastRuntime = _dateTimeService.NowDatabaseTime;
            await UpdateCurrentResourceUsage();

            var millisecondsPassed = (_dateTimeService.NowDatabaseTime - _lastRuntime).Milliseconds;
            if (millisecondsPassed < _generalConfig.Value.HostWorkIntervalMs)
                await Task.Delay(_generalConfig.Value.HostWorkIntervalMs - millisecondsPassed, stoppingToken);
        }
        
        _logger.Debug("Stopping {ServiceName} service", nameof(HostWorker));
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
            IpAddresses = adapter.IPAddressList,
            DefaultGateways = adapter.DefaultIPGatewayList,
            DhcpServer = adapter.DHCPServer,
            DnsServers = adapter.DNSServerSearchOrderList
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

        var serializedRequest = _serializerService.Serialize(hostDetailRequest);
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = 0,
            Type = HostWorkType.HostDetail,
            Status = WeaverWorkState.Completed,
            WorkData = serializedRequest,
            AttemptCount = 0
        });
    }

    private async Task UpdateCurrentResourceUsage()
    {
        if ((_dateTimeService.NowDatabaseTime - CurrentHostResourceUsage.TimeStamp).Seconds > 10)
        {
            _logger.Warning("Last resource gather was over 10 seconds ago, the poller must have died, restarting the poller");
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
}