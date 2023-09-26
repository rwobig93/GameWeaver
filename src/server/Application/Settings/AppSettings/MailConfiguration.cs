namespace Application.Settings.AppSettings;

public class MailConfiguration : IAppSettingsSection
{
    public const string SectionName = "Mail";
    
    public string? From { get; init; }
    
    public string? Host { get; init; }
    
    public int Port { get; init; }
    
    public string? UserName { get; init; }
    
    public string? Password { get; init; }
    
    public string? DisplayName { get; init; }
}