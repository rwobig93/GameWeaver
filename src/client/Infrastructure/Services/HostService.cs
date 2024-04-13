using Application.Services;
using Domain.Models.Host;
using Hardware.Info;

namespace Infrastructure.Services;

public class HostService : IHostService
{
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public HostService(ILogger logger, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    private static readonly IHardwareInfo HardwareInfo = new HardwareInfo(useAsteriskInWMI: false);

    public void PollHostDetail()
    {
        try
        {
            _logger.Verbose("Gathering current host detail");

            HardwareInfo.RefreshOperatingSystem();
            HardwareInfo.RefreshMotherboardList();
            HardwareInfo.RefreshVideoControllerList();
            HardwareInfo.RefreshDriveList();
            HardwareInfo.RefreshMemoryList();
            
            _logger.Verbose("Successfully gathered current host detail");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred gathering host resource usage: {Error}", ex.Message);
        }
    }

    public void PollHostResources()
    {
        try
        {
            _logger.Verbose("Gathering current host resource usage");

            HardwareInfo.RefreshMemoryStatus();
            HardwareInfo.RefreshCPUList();
            HardwareInfo.RefreshNetworkAdapterList(includeBytesPerSec: true);
            
            _logger.Verbose("Successfully gathered current host resource usage");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred gathering host resource usage: {Error}", ex.Message);
        }
    }

    public HostResourceUsage GetCurrentResourceUsage()
    {
        var hostResourceUsage = new HostResourceUsage();

        try
        {
            var totalCpuUsage = HardwareInfo.CpuList.Sum(socket => (double)socket.PercentProcessorTime);
            var cpuUsage = totalCpuUsage / HardwareInfo.CpuList.Count;
        
            var ramUsed = HardwareInfo.MemoryStatus.TotalPhysical - HardwareInfo.MemoryStatus.AvailablePhysical;
            var ramUsage = (double) ramUsed / HardwareInfo.MemoryStatus.TotalPhysical;

            var totalNetworkInBytes = HardwareInfo.NetworkAdapterList.Sum(netInterface => (double)netInterface.BytesReceivedPersec);
            var totalNetworkOutBytes = HardwareInfo.NetworkAdapterList.Sum(netInterface => (double)netInterface.BytesSentPersec);

            hostResourceUsage.CpuUsage = cpuUsage;
            hostResourceUsage.RamUsage = ramUsage * 100;
            hostResourceUsage.Uptime = Environment.TickCount64;
            hostResourceUsage.NetworkOutBytes = totalNetworkOutBytes;
            hostResourceUsage.NetworkInBytes = totalNetworkInBytes;
            hostResourceUsage.TimeStamp = _dateTimeService.NowDatabaseTime;
            
            _logger.Verbose("Successfully gathered host resource usage: {CpuUsage} | {RamUsage} | {Uptime} | {NetInMb} | {NetOutMb}",
                hostResourceUsage.CpuUsage, hostResourceUsage.RamUsage, TimeSpan.FromMilliseconds(hostResourceUsage.Uptime), hostResourceUsage.NetworkInBytes, hostResourceUsage.NetworkOutBytes);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred serializing host resource usage: {Error}", ex.Message);
        }

        return hostResourceUsage;
    }

    public IHardwareInfo GetHardwareInfo()
    {
        return HardwareInfo;
    }
}