namespace Application.Settings;

public class AuthConfiguration : IAppSettingsSection
{
    public const string SectionName = "Auth";
    
    public string RegisterUrl { get; init; } = "https://localhost:9500";

    public string Host { get; init; } = "";

    public string Key { get; init; } = "";
}