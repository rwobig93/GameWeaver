using Domain.Models.Host;
using Hardware.Info;

namespace Application.Services;

public interface IHostService
{
    void PollHostDetail();
    void PollHostResources();
    HostResourceUsage GetCurrentResourceUsage();
    IHardwareInfo GetHardwareInfo();
    string GetHostname();
}