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
}