namespace Application.Constants;

public class HostConstants
{
#if DEBUG
    public const string ConfigFile = "appsettings.Development.json";
#else
    public const string ConfigFile = "appsettings.json";
#endif
    public const string QueryHostId = "hostId";
    public const string QueryHostRegisterKey = "registerKey";
    public const string InProgressQueuePath = "host-work-inprogress.json";
    public const string WaitingQueuePath = "host-work-waiting.json";
}