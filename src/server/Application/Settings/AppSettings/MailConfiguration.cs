using System.ComponentModel.DataAnnotations;

namespace Application.Settings.AppSettings;

public class MailConfiguration : IAppSettingsSection
{
    public const string SectionName = "Mail";

    public string? From { get; init; }

    public string? Host { get; init; }

    [Range(1, 65_535)]
    public int Port { get; init; } = 465;

    public string? UserName { get; init; }

    public string? Password { get; init; }

    public string? DisplayName { get; init; }
}